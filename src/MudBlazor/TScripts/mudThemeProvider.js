const darkThemeMediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
let dotNetHelper;

window.darkModeChange = (dotNetHelper) => {
    return darkThemeMediaQuery.matches;
};

function watchDarkThemeMedia(dotNet) {
    if (dotNet === undefined){
        console.WriteLine("dotNetHelper is broken")
    }
    dotNetHelper = dotNet;
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function(e) { 
        dotNetHelper.invokeMethodAsync('SystemPreferenceChanged', darkThemeMediaQuery.matches); 
    });
}