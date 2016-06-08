function RefreshOn() {
  console.log('refresh on');
  $("#refresh-spinner").removeClass('hide');
}

function RefreshOff() {
  console.log('refresh off');
  $("#refresh-spinner").addClass('hide');
}