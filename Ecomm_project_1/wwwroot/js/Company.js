var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "lengthMenu": [2,4,6,8,10],
        "ajax": {
            "url": "/Admin/Company/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "streetAddress", "width": "15%" },
            { "data": "city", "width": "15%" },
            { "data": "state", "width": "15%" },            
            { "data": "phoneNumber", "width": "15%" },
            {
                "data": "isAuthorizedCompany",
                "render": function (data) {                    
                    if(data)
                    {
                      return `<input type="checkbox" disabled  checked />`
                    }
                    else
                    {
                       return `<input type="checkbox" disabled />`
                    }
                   
                }, "width": "5%"                
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="text-center">
                    <a href="/Admin/Company/Upsert/${data}" class="btn btn-info">
                    <i class="fas fa-edit"></i></a>
                    <a class="btn btn-danger" onclick=Delete('/Admin/Company/Delete/${data}')>
                    <i class="fas fa-trash-alt"></i></a>
                    </div>
                    `;
                }
            }
        ]
    })
}

function Delete(url) {
    // alert(url);
    swal({
        title: "Want To Delete Data?",
        icon: "/images/3.png",
        buttons: true,
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