<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<AdvancedTask.Models.AdvancedTaskIndexViewData>" %>
<%@ Import Namespace="AdvancedTask.Business.AdvancedTask" %>
<%@ Import Namespace="AdvancedTask.Models" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.Editor" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>

<% Html.RenderPartial("Menu", "ChangeApproval"); %>
<style>
    .inner-table {
        border-collapse: collapse !important;
        /*border: none !important;*/
        empty-cells: show !important;
        width: 100% !important;
        height: 100% !important;
        border-bottom: 1pt solid #aeaeae;
        /*border-spacing: 0 !important;*/
    }

        .inner-table .short {
            width: 20% !important;
        }

        .inner-table td {
            padding: 6px !important;
        }

        .inner-table .header {
            border-bottom: 1pt solid #aeaeae !important;
        }

        .inner-table .header-right {
            border-right: 1pt solid #aeaeae !important;
        }

        .inner-table .epi-changeapproval-faded {
            color: #818181 !important;
        }

    .changetask-detail {
        background-color: #d9edf7 !important;
    }

    .change-anchor {
        text-decoration: underline !important;
        color: navy !important;
    }
</style>

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
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="name_desc"?"name_aes":"name_desc",isChange = true})%>
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
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="ctype_desc"?"ctype_aes":"ctype_desc",isChange = true})%>
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
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="type_aes"?"type_aes":"type_desc",isChange = true})%>
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
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="timestamp_desc"?"timestamp_aes":"timestamp_desc",isChange = true})%>
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
                                new { pageNumber = Model.PageNumber, pageSize = Model.PageSize, sorting = Model.Sorting=="user_desc"?"user_aes":"user_desc",isChange = true})%>
                </label>
            </th>
        </tr>
    </thead>

    <% if (Model.ContentTaskList.Count > 0)
        {
            foreach (ContentTask m in Model.ContentTaskList)
            {
    %>
    <tr <%= m.NotificationUnread ? "style=\"background-color: #FFF9C4;cursor: pointer;\"" : "cursor: pointer" %> class="parent" title="Click to expand/collapse" id="<%= m.ApprovalId %>">
        <td><%= Html.CheckBox(m.ApprovalId.ToString(), false, new {onchange = "selectionChanged(this)", @class = "checkbox"}) %></td>
        <td>
            <%= m.ContentName %>
        </td>
        <td>
            <%= m.ContentType %>
        </td>
        <td>
            <%= m.Type %>
        </td>
        <td>
            <%= m.DateTime %>
        </td>
        <td>
            <%= m.StartedBy %>
        </td>
    </tr>
    <% if (m.Details != null && m.Details.Any())
        { %>
    <tr class="child-<%= m.ApprovalId %>" style="display: none;">
        <td class="changetask-detail" colspan="6">
            <table class="inner-table">
                <thead>
                    <tr class="header">
                        <td class="short header-right"><strong>Name</strong></td>
                        <td class="header-right"><strong>Current Version</strong></td>
                        <td><strong>Suggested Version</strong></td>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var ct in m.Details.ToList())
                        {
                    %>
                    <tr>
                        <td class="short"><%= ct.Name %></td>
                        <td><%= ct.OldValue %></td>
                        <td><%= ct.NewValue %></td>
                    </tr>
                    <% }
                        } %>
                </tbody>
            </table>
            <p style="padding-top: 20px"><a class="change-anchor" href="<%=m.URL%>" target="_blank">Go to the Change Details</a></p>
        </td>
    </tr>
    <% }
        } %>
</table>

<div class="epi-formArea" id="approve" style="display: none;">
    <fieldset>
        <% Html.BeginGadgetForm("Index"); %>
        <p>Approve Entire Approval Sequence</p>

        <textarea autocomplete="off" cols="70" id="approvalComment" name="approvalComment" class="epi-textarea--max-height--500 dijitTextBox dijitTextArea dijitExpandingTextArea dijitTextBoxError dijitTextAreaError dijitExpandingTextAreaError dijitError" tabindex="0" placeholder="Please specify why you are forcing approval of the content…" rows="1" style="overflow: auto hidden; box-sizing: border-box; height: 29px;" spellcheck="false"></textarea>
        <input type="hidden" name="pageSize" value="<%=Model.PageSize %>" />
        <input id="taskValues" type="hidden" name="taskValues" value="<%=Model.TaskValues%>" />
        <input type="hidden" name="isChange" value="<%=true%>" />
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

    $(document).ready(function () {
        $('tr.parent')
            .css("cursor", "pointer")
            .attr("title", "Click to expand/collapse")
            .click(function () {
                $(this).siblings('.child-' + this.id).toggle();
            });
        //$('tr[@class^=child-]').hide().children('td');
    });    
</script>

