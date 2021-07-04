$(document).ready(function () {
    loadProduct();
});
function loadProduct() {
    $.ajax({
        url: "TestAjax/getProducts",
        type: "GET",
        contentType: "application/json;charset=utf-8",
        success: function (result) {
            var html = '';
            $each(result, function(key, item)){
            html += '<tr>';
            html += '<td>' + item.id;
            }
        }
    });
}