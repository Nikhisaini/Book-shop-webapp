var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/AllOrder/GetOrderDetails/" + orderId
        },
        "columns": [
            { "data": "productId", "width": "10%" },
            { "data": "product.title", "width": "25%" },
            { "data": "product.author", "width": "15%" },
            { "data": "price", "width": "10%" },
            { "data": "count", "width": "10%" },
            {
                "data": "id",
                "render": function (data, type, row) {
                    var amount = row.price * row.count;
                    return `<span>₹${amount.toFixed(2)}</span>`;
                },
                "width": "15%"
            }
        ]
    });
}