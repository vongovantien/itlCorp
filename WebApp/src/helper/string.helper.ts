export function subStringComma(textString: string) {
    if (textString == null) { return textString; }
    if (textString.length <= 0) { return textString; }
    if (textString[textString.length - 2] === ',') {
        textString = textString.substring(0, (textString.length - 2));
    }
    return textString;
}