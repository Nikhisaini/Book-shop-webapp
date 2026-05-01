$(document).ready(() => loadMonthData()); // Load data on startup

function loadMonthData() {
    const monthVal = parseInt($('#monthSelect').val());
    const $container = $('#tablesContainer').html("<p class='text-center mt-4'>Loading...</p>");

    // 1. Fetch data using shorthand $.get
    $.get(`/Admin/AllOrder/GetByMonth?month=${monthVal}`)
        .done(res => renderOrders(res.data, monthVal, $container))
        .fail(() => $container.html("<p class='text-danger text-center'>Failed to load data.</p>"));
}

function renderOrders(data, selectedMonth, $container) {
    $container.empty();
    const currentYear = new Date().getFullYear();
    const monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

    // 2. Decide what to loop through: All 12 months, or just the 1 selected month
    const monthsToRender = selectedMonth === 0
        ? monthNames.map((name, index) => ({ name, index }))
        : [{ name: monthNames[selectedMonth - 1], index: selectedMonth - 1 }];

    // 3. Loop through the required months and build the UI
    monthsToRender.forEach(({ name, index }) => {
        const title = `${name} ${currentYear}`;

        // Filter orders for this specific loop's month
        const monthData = data.filter(order => new Date(order.orderDate).getMonth() === index);

        if (monthData.length > 0) {
            buildTable(title, monthData, $container);
        } else if (selectedMonth === 0) {
            // Show "No record" empty state only when viewing All Months
            $container.append(`
                <div class="mb-5 shadow-sm p-4 bg-white rounded">
                    <h4 class="text-primary mb-3 border-bottom pb-2">${title}</h4>
                    <div class="alert alert-light text-center text-muted border py-3">No order record found.</div>
                </div>`);
        } else {
            // Simple text if a single month is filtered and has no data
            $container.html(`<h5 class='text-center mt-4 text-muted'>No orders found for ${title}.</h5>`);
        }
    });
}

function buildTable(title, data, $container) {
    const tableId = `tbl_${title.replace(/\s+/g, '')}`;

    // Inject Table HTML
    $container.append(`
        <div class="mb-5 shadow-sm p-4 bg-white rounded">
            <h4 class="text-primary mb-3 border-bottom pb-2">${title}</h4>
            <div class="table-responsive">
                <table id="${tableId}" class="table table-hover align-middle w-100">
                    <thead class="bg-light text-secondary small text-uppercase fw-bold">
                        <tr><th>ID</th><th>Date</th><th>Customer</th><th>Status</th><th>Total Amount</th><th class="text-center">Action</th></tr>
                    </thead>
                </table>
            </div>
        </div>`);

    // Initialize DataTable on the injected HTML
    $(`#${tableId}`).DataTable({
        data: data,
        order: [[1, 'desc']], // Sort by date descending
        columns: [
            { data: "id", width: "5%" },
            { data: "orderDate", width: "20%" },
            { data: "name", width: "25%" },
            { data: "orderStatus", width: "10%" },
            { data: "orderTotal", width: "15%", render: amt => `₹${amt}` },
            {
                data: "id", width: "15%", className: "text-center", render: id =>
                    `<a href="/Admin/AllOrder/Details/${id}" class="btn btn-sm btn-info text-white">View Detail</a>`
            }
        ]
    });
}