var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            // IMPORTANT: Make sure this URL returns a list of PRODUCTS, not Categories!
            // Example: "url": "/Customer/Home/GetAll"
            "url": "/Customer/Home/GetAll"
        },
        "columns": [
            {
                // We use 'null' so the 'row' parameter gets the entire JSON object
                "data": null,
                "render": function (data, type, row) {
                    // Use standard JavaScript ${row.propertyName} instead of Razor @product
                    return `
                        <div class="card p-2" style="border:1px solid #008cba; border-radius: 5px; width: 100%;">
                            <img src="${row.imageUrl}" class="card-img-top rounded" style="max-height: 300px; object-fit: cover;" />
                            <div class="pl-1 mt-2">
                                <p class="card-title h5"><b style="color:#2c3e50">${row.title}</b></p>
                                <p class="card-title text-primary">by <b>${row.author}</b></p>
                            </div>
                            <div style="padding-left:5px;">
                                <p>List Price: <strike><b class="">$${row.listPrice.toFixed(2)}</b></strike></p>
                            </div>
                            <div style="padding-left:5px;">
                                <p style="color:maroon">As low as: <b class="">$${row.price100.toFixed(2)}</b></p>
                            </div>
                            <div>
                                <a href="/Customer/Home/Details/${row.id}" class="btn btn-primary form-control">Details</a>
                            </div>
                        </div>
                    `;
                }
            }
        ]
    });
}