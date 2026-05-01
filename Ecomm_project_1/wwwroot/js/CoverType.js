var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "lengthMenu":[2,4,6,8,10],
        "ajax": {
            "url": "/Admin/CoverType/GetAll"
        },
        "columns": [
            { "data": "name", "width": "70%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="text-center">
                    <a href="/Admin/CoverType/Upsert/${data}" class="btn btn-info">
                    <i class="fas fa-edit"></i></a>
                    <a class="btn btn-danger" onclick=Delete('/Admin/CoverType/Delete/${data}')>
                    <i class="fas fa-trash-alt"></i></a>
                    </div>
                    `;
                }
            }
        ]
    })
}

function Delete(url) {
    //  alert(url);
    swal({
        title: "Do you really want to delete the data?",
        icon: "/images/3.png",
        buttons: true, 
        className:"blue-border-popup",
        dangerModel: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}