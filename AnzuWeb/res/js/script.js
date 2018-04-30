$(document).ready(function ()
{
	var searchBox = $("input[type=search]");
	var form = $("form");
	var content = $("#content");
	var lastQuery = "";
	var lastReq;
	form.submit(function(e){
		e.preventDefault();
		searchBox.trigger("change");
		return false;
	});
	searchBox.on("propertychange change keyup input paste", function (e) {
		e.preventDefault();
		if(searchBox.val() == "" || searchBox.val() == " ")
		{
			window.history.pushState("Anzu", "Anzu Search", '/');
			content.html("");
			return false;
		}
		if(lastQuery == searchBox.val())
		return false;
		lastQuery = searchBox.val();
		window.history.pushState("Anzu " + searchBox.val(), "Anzu Search", '/s/' + encodeURI(searchBox.val()));
		var url = "/search/" + searchBox.val();
		console.log(url);
		if(lastReq != null || lastReq != undefined)
			lastReq.abort();
		lastReq = $.ajax({
			url : url
		}).done(function(data){
			let html = "";
			for (let i = 0; i < data.length; i++) {
				const element = data[i];
				html += "<div class='Image'>"+ element.name  +"</div>"
			}
			content.html(html);
			console.log(html);
		});
	});
	searchBox.trigger("change");
});