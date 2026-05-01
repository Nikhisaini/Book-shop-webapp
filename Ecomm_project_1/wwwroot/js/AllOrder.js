var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/AllOrder/GetAll"
        },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "orderDate", "width": "20%" },
            { "data": "name", "width": "25%" },
            { "data": "orderStatus", "width": "10%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/AllOrder/Details/${data}" class="btn btn-info text-white" style="cursor:pointer">
                            View Detail
                            </a>
                            
                           `;
                }
            }
        ],
        "footerCallback": function (row, data, start, end, display) {
            var api = this.api();

            var total = api.column(4).data().reduce((a, b) => a + b, 0);
            $(api.column(4).footer()).html('₹' + total.toFixed(2));
        }
    });
}
