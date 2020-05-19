﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<AdvancedTask.Models.AdvancedTaskIndexViewData>" %>
<%@ Import Namespace="AdvancedTask.Business.AdvancedTask" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.Editor" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>

<% Html.RenderPartial("Menu", "Index"); %>
<table class="epi-default">
    <thead>
        <tr>
            <th>
                <input type="checkbox" onchange="checkAll(this)" name="chk[]" id="chk" /></th>
            <th>
                <label>
                    <%= Html.ViewLink(
                                "Content Name",
                                "Content Name",  // title
                                "Index", // Action name
                                "", // css class
                                "",
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="name_desc"?"name_aes":"name_desc"})%>
                </label>
            </th>
            <th>
                <label>
                    <%= Html.ViewLink(
                                "Content Type",
                                "Content Type",  // title
                                "Index", // Action name
                                "", // css class
                                "",
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="ctype_desc"?"ctype_aes":"ctype_desc"})%>
                </label>
            </th>
            <th>
                <label>
                    <%= Html.ViewLink(
                                "Type",
                                "Type",  // title
                                "Index", // Action name
                                "", // css class
                                "",
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="type_aes"?"type_aes":"type_desc"})%>
                </label>
            </th>
            <th>
                <label>
                    <%= Html.ViewLink(
                                "Submitted Date/Time",
                                "Submitted Date/Time",  // title
                                "Index", // Action name
                                "", // css class
                                "",
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="timestamp_desc"?"timestamp_aes":"timestamp_desc"})%>
                </label>
            </th>
            <th>
                <label>
                    <%= Html.ViewLink(
                                "Started By",
                                "Started By",  // title
                                "Index", // Action name
                                "", // css class
                                "",
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="user_desc"?"user_aes":"user_desc"})%>
                </label>
            </th>
        </tr>
    </thead>

    <% if (Model.ContentTaskList.Count > 0)
        {
            foreach (ContentTask m in Model.ContentTaskList)
            {
    %>
    <tr <%=m.NotificationUnread?"style=\"background-color: #FFF9C4;\"" :"" %>>
        <% if (!string.IsNullOrEmpty(m.ApprovalType) && m.ApprovalType.Equals("Content"))
            { %>
        <td><%=Html.CheckBox(m.ApprovalId.ToString(), false, new { onchange = "selectionChanged(this)", @class="checkbox" })%></td>
        <% }
            else
            { %>
        <td></td>
        <% } %>
        <td>
            <% if (!ContentReference.IsNullOrEmpty(m.ContentReference))
                {
                    if (!m.CanUserPublish)
                    { %>
            <a href="<%= PageEditing.GetEditUrl(m.ContentReference) %>" id="id-<%= m.ApprovalId.ToString() %>" data-value="ID: <%= m.ContentReference.ID %> - <%= m.ContentName %>" target="_blank"><%= Html.Encode(m.ContentName) %>
                <span style="color: red" class="error-span" id="span-<%= m.ApprovalId.ToString() %>"></span>
            </a>
            <% }
                else
                { %>
            <a href="<%= PageEditing.GetEditUrl(m.ContentReference) %>" target="_blank"><%= Html.Encode(m.ContentName) %></a>
            <% }
                }
                else
                { %>
            <%= m.ContentName%>
            <% } %>
        </td>
        <td>
            <% if (ContentReference.IsNullOrEmpty(m.ContentReference))
                { %>
            <%= m.ContentType%>
            <% }
                else
                { %>
            <a href="<%= PageEditing.GetEditUrl(m.ContentReference) %>" target="_blank"><%= Html.Encode(m.ContentType) %></a>
            <% } %>
        </td>
        <td>
            <% if (ContentReference.IsNullOrEmpty(m.ContentReference))
                { %>
            <%= m.Type%>
            <% }
                else
                { %>
            <a href="<%= PageEditing.GetEditUrl(m.ContentReference) %>" target="_blank"><%= Html.Encode(m.Type) %></a>
            <% } %>
        </td>
        <td>
            <% if (ContentReference.IsNullOrEmpty(m.ContentReference))
                { %>
            <%= m.DateTime%>
            <% }
                else
                { %>
            <a href="<%= PageEditing.GetEditUrl(m.ContentReference) %>" target="_blank"><%= Html.Encode(m.DateTime) %></a>
            <% } %>
        </td>
        <td>
            <% if (ContentReference.IsNullOrEmpty(m.ContentReference))
                { %>
            <%= m.StartedBy%>
            <% }
                else
                { %>
            <a href="<%= PageEditing.GetEditUrl(m.ContentReference) %>" target="_blank"><%= Html.Encode(m.StartedBy) %></a>
            <% } %>
        </td>
    </tr>
    <%} %>
    <%
        } %>
</table>

<div class="epi-formArea" id="approve" style="display: none;">
    <fieldset>
        <% Html.BeginGadgetForm("Index"); %>
        <p>Approve Entire Approval Sequence</p>

        <textarea autocomplete="off" cols="70" id="approvalComment" name="approvalComment" class="epi-textarea--max-height--500 dijitTextBox dijitTextArea dijitExpandingTextArea dijitTextBoxError dijitTextAreaError dijitExpandingTextAreaError dijitError" tabindex="0" placeholder="Please specify why you are forcing approval of the content…" rows="1" style="overflow: auto hidden; box-sizing: border-box; height: 29px;" spellcheck="false"></textarea>
        <input type="hidden" name="pageSize" value="<%=Model.PageSize %>" />
        <input id="taskValues" type="hidden" name="taskValues" value="<%=Model.TaskValues%>" />
        <div>
            <ul>
                <li>
                    <button type="submit" class="taskbutton search" id="button" onclick="return Submit();">Submit</button>
                </li>
            </ul>
        </div>
        <% Html.EndForm(); %>
    </fieldset>
</div>
<br />
<script type="text/javascript">
    var selectedContent = [];

    function selectionChanged(element) {
        if (element.checked && element.id !== 'chk') {
            if (!selectedContent.includes(element.name)) {
                selectedContent.push(element.name);
            }
        } else {
            var index = selectedContent.indexOf(element.name);
            if (index > -1 && element.id !== 'chk') {
                removeA(selectedContent, element.name);

                var checkbox = document.getElementById('chk');
                if (selectedContent.length === 0 && checkbox.checked) {
                    checkbox.checked = false;
                }
            }

            var name = element.name;
            var element1 = document.getElementById('span-' + name);
            if (element1) {
                element1.textContent = "";
            }
        }
        setButtonText();
        setCheckboxSelectedValues();
    }

    function setCheckboxSelectedValues() {
        var value = '';
        selectedContent.forEach(function (content) {
            value = value + ',' + content;
        });

        document.getElementById('taskValues').value = value;
    }

    function checkAll(ele) {
        var checkboxes = document.getElementsByClassName('checkbox');
        if (ele.checked) {
            for (var i = 0; i < checkboxes.length; i++) {
                if (checkboxes[i].type === 'checkbox') {
                    checkboxes[i].checked = true;
                    selectionChanged(checkboxes[i]);
                }
            }
        } else {
            for (var j = 0; j < checkboxes.length; j++)
                if (checkboxes[j].type === 'checkbox') {
                    checkboxes[j].checked = false;
                    selectionChanged(checkboxes[j]);
                }
        }
    }

    function removeA(arr) {
        var what, a = arguments, L = a.length, ax;
        while (L > 1 && arr.length) {
            what = a[--L];
            while ((ax = arr.indexOf(what)) !== -1) {
                arr.splice(ax, 1);
            }
        }
        return arr;
    }

    function Submit() {
        var comment = document.getElementById("approvalComment").value;
        if (comment === "" || comment.length === 0) {
            alert("Add comment.");
            return false;
        } else {
            var message = "Are you sure that you want to approve the entire approval sequence? This will approve all remaining steps. This action cannot be undone.";
            if (confirm(message)) {
                return true;
            } else {
                return false;
            }
        }
    }

    function setButtonText() {
        if (selectedContent && selectedContent.length > 0) {
            ShowApproveSection();

                document.getElementById('button').textContent =
                    'Approve ' + selectedContent.length + ' Selected Content Changes';
        } else {
            HideApproveSection();
            document.getElementById('button').textContent = 'Submit';
        }
    }

    function checkboxChecked(checkboxElem) {
        if (checkboxElem.checked) {
            checkboxElem.value = "true";
        } else {
            checkboxElem.value = "false";
            var elements = document.getElementsByClassName('error-span');
            for (var i = 0; i < elements.length; i++) {
                elements[i].innerHTML = "";
            }
        }
        setButtonText();
    }

    var coll = document.getElementsByClassName("collapsible");
    var i;

    if (coll) {
        for (i = 0; i < coll.length; i++) {
            coll[i].addEventListener("click",
                function () {
                    this.classList.toggle("active");
                    var content = this.nextElementSibling;
                    if (content.style.display === "block") {
                        content.style.display = "none";
                    } else {
                        content.style.display = "block";
                    }
                });
        }
    }

    function ShowApproveSection() {
        var x = document.getElementById("approve");
        if (x) {
            x.style.display = "block";
        }
    }

    function HideApproveSection() {
        var x = document.getElementById("approve");
        if (x) {
            x.style.display = "none";
        }
        if (document.getElementById("lblMessage")) {
            document.getElementById("lblMessage").innerHTML = "";
        }
    }


</script>

