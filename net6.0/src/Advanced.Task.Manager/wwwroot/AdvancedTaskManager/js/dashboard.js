(function () {
    'use strict';

    feather.replace();

    $(function () {
        $("[data-toggle='tooltip']").tooltip();
    });

    $(document).ready(function () {
        $('.atm-more').click(function (e) {
            e.stopPropagation(); // Prevent the click event from reaching the document

            var tooltip = $(this).siblings('.atm-tooltip');
            $('.atm-tooltip').not(tooltip).hide(); // Hide other tooltips
            tooltip.show();
        });

        $('.atm-editButton').click(function (e) {
            e.stopPropagation(); // Prevent the click event from reaching the document
            var url = $(this).data('url');
            window.open(url, '_blank');
            $('.atm-tooltip').hide();
        });

        // Handle clicks outside of tooltips to hide them
        $(document).click(function () {
            $('.atm-tooltip').hide();
        });

        $('.dropdown-toggle').dropdown();
    });
})()