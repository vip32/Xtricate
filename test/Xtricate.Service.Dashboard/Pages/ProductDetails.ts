/// <reference path="../Typings/jquery.d.ts"/>
/// <reference path="../Typings/knockout.d.ts"/>

setTimeout(() => console.log('hello from ProductDetails.ts'));

var viewModel: any = {
    isLoading: ko.observable(false),
    name: ko.observable(),
    description: ko.observable(),
    sku: ko.observable()
};
ko.applyBindings(viewModel);

$(document).ready(() => {
    refresh();
});

$('#refresh').click(() => {
    refresh();
    //$('.tree').treegrid();
});

function refresh(): void {
    console.log('refresh');
    viewModel.isLoading(true);
    $.getJSON("/api/products/" + window.location.href.substr(window.location.href.lastIndexOf('/') + 1), data => {
        //console.log(data);
        viewModel.isLoading(false);
        viewModel.name(data.name);
        viewModel.description(data.description);
        viewModel.sku(data.sku);
    });
};