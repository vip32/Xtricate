/// <reference path="../Typings/jquery.d.ts"/>
/// <reference path="../Typings/knockout.d.ts"/>

setTimeout(() => console.log('hello from ProductIndex.ts'));

var data = [];
var viewModel = {
    isLoading: ko.observable(false),
    products: ko.observableArray(data)
};
ko.applyBindings(viewModel);

$(document).ready(() => {
    refresh();
});

console.log(sprintf("%2$s %3$s a %1$s", "cracker", "Polly", "wants"));
console.log(sprintf("Current timestamp: %d", Date.now)); // Current timestamp: 1398005382890
console.log(sprintf("Current date and time: %s", () => new Date().toString()));

$('#refresh').click(() => {
    refresh();
    //$('.tree').treegrid();
});

function refresh(): void {
    console.log('refresh');
    viewModel.products(null);
    viewModel.isLoading(true);
    $.getJSON("/api/products", data => {
        //console.log(data);
        viewModel.isLoading(false);
        viewModel.products(data);
    });
};