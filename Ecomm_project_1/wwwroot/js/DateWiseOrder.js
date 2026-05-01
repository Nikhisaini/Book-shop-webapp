var dataTable;

$(document).ready(function () {
    $('#tableContainer').hide();

    $('#fromDate').on('change', function () {
        var fromDateValue = $(this).val();

        $('#toDate').attr('min', fromDateValue);

        var currentToDate = $('#toDate').val();
        if (currentToDate && currentToDate < fromDateValue) {
            $('#toDate').val('');
        }
    });
});
function loadDataTable(start = '', end = '', status = 'All') {
    if ($.fn.DataTable.isDataTable('#tblData')) {
        $('#tblData').DataTable().destroy();
    }

    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/AllOrder/GetSearchDate",
            "data": {
                start: start,  end: end,  status: status 
            }
        },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "orderDate", "width": "20%" },
            { "data": "name", "width": "25%" },
            { "data": "orderStatus", "width": "10%" }, 
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<a href="/Admin/AllOrder/Details/${data}" class="btn btn-info">View Detail</a>`;
                }
            }
        ]
    });
}
function filterByDate() {
    var fromDate = $('#fromDate').val();
    var toDate = $('#toDate').val();
    var status = $('#status').val();   

    $('#tableContainer').show();
    loadDataTable(fromDate, toDate, status);
}