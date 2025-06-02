$(document).ready(function ($) {
    var rules;
    for (var i = document.styleSheets.length - 1; i >= 0; i--) {
        if (document.styleSheets[i].cssRules)
            rules = document.styleSheets[i].cssRules;
        else if (document.styleSheets[i].rules)
            rules = document.styleSheets[i].rules;
        for (var idx = 0, len = rules.length; idx < len; idx++) {
            if (rules[idx].selectorText.indexOf("hover") == -1) {
                $(rules[idx].selectorText).each(function (i, elem) {
                    elem.style.cssText = rules[idx].style.cssText + elem.style.cssText;
                });
            }
        }
    }
    $('style').remove();
    $('script').remove();
    $('link').remove();
});