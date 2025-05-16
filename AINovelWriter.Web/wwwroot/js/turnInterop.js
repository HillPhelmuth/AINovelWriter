let isTurning = false;
window.init = (dotNetRef) => {
    var query = window.matchMedia("(max-width: 899px)");
    var singleDouble = query.matches ? "single" : "double";
    if ($(".flipbook").turn("is")) {
        $(".flipbook").turn("destroy");
    }
    $(".flipbook").turn({
        // Elevation
        elevation: 50,
        height: 500,
        // Enable gradients
        gradients: true,
        // Auto center this flipbook
        autoCenter: true,
        display: singleDouble,
        when: {
            //start: function (event, pageObject) {

            //    isTurning = true;
            //},
            end: function (event, pageObject, turned) {
                if (!isTurning) {
                   
                    var currentPage = $(".flipbook").turn("page");
                    dotNetRef.invokeMethodAsync('OnTurnEnd', currentPage);
                    console.log(`Page Flip End event sent to dotnet: ${currentPage}`);
                }
                else {
                    isTurning = false;
                }
            }
        }
    });
}
window.next = () => {
    isTurning = true;
    $(".flipbook").turn("next");
    return $(".flipbook").turn("page");
}
window.back = () => {
    isTurning = true;
    $(".flipbook").turn("previous");
    return $(".flipbook").turn("page");
}
window.turnToPage = (page) => {
    isTurning = true;
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
window.setDisplayMode = (mode) => {
    console.log(`Setting display mode to: ${mode}`);
    var width = mode === "single" ? 450 : 900;
    $(".flipbook").turn("display", mode); // Switch to single page view
    $(".flipbook").turn("size", width, 500);

}
window.zoom = (zoom) => {
    console.log(`Setting zoom to: ${zoom}`);
   
    $(".flipbook").turn("zoom", zoom); 
    
}
