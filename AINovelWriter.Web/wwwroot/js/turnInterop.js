window.init = () => {
	var query = window.matchMedia("(max-width: 899px)");
	var singleDouble = query.matches ? "single" : "double";
	if ($(".flipbook").turn("is")) {
		$(".flipbook").turn("destroy");
	}
	$(".flipbook").turn({

		// Elevation

		elevation: 50,

		// Enable gradients

		gradients: true,

		// Auto center this flipbook

		autoCenter: true,
		display: singleDouble,

	});
}
window.next = () => {
	$(".flipbook").turn("next");
	return $(".flipbook").turn("page");
}
window.back = () => {
	$(".flipbook").turn("previous");
	return $(".flipbook").turn("page");
}
window.turnToPage = (page) => {
	if ($(".flipbook").turn("hasPage", page)) {
		$(".flipbook").turn("page", page);
	}
}
window.resizeBook = () => {
	$(".flipbook").turn("resize");
}
window.pageCount = () => {
	return $(".flipbook").turn("pages");
}
window.checkSize = () => {
	var query = window.matchMedia("(max-width: 640px)");
	return query.matches;
}
window.checkHeight = () => {
	var query = window.matchMedia("(max-height:550px)");
	return query.matches;
}