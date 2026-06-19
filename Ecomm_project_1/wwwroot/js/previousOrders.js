var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblPreviousOrders').DataTable({
        "lengthMenu": [5, 10, 15, 20],
        "ajax": {
            "url": "/Customer/Order/GetAllPreviousOrders"
        },
        "columns": [
            { "data": "orderId", "width": "10%" },
            { "data": "name", "width": "20%" },
            { "data": "state", "width": "15%" },
            { "data": "productName", "width": "35%" },
            {
                "data": "orderId",
                "render": function (data, type, row) {
                    return `
            <div class="text-center">
                <a href="/Customer/Order/Details?orderId=${row.orderId}&productId=${row.productId}" class="btn" style="background-color: #8bc34a; color: white;">
                    VIEW DETAIL
                </a>
            </div>
        `;
                },
                "width": "20%"
            }
        ]
    });
}