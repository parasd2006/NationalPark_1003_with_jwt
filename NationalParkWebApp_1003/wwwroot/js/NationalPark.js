var dataTable;

$(document).ready(function () {
    $.fn.DataTable.ext.pager.numbers_length = 5;
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/NationalPark/GetAll"
        },
        "columns": [
            { "data": "name", "width": "40%" },
            { "data": "state", "width": "35%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="text-center">
                            <a href="/NationalPark/Upsert/${data}" class="btn btn-info text-white" style="cursor:pointer">
                                <i class="fas fa-edit"></i>
                            </a>
                            <a class="btn btn-danger text-white" style="cursor:pointer" onclick="Delete('/NationalPark/Delete/${data}')">
                                <i class="fas fa-trash"></i>
                            </a>
                        </div>
                    `;
                },
                "width": "25%"
            }
        ]
    });
}

function Delete(url) {
    swal({
        title: "Are you sure you want to Delete?",
        text: "You will not be able to restore the data!",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}