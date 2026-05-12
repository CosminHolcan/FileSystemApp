import { IStyleFunctionOrObject, ITextFieldStyleProps, ITextFieldStyles } from "@fluentui/react";


export const baseTextFieldFieldStyles = {
    flex: '1',
    padding: '10px',
    border: '1px solid #ccc',
    borderRadius: '4px',
    fontSize: '16px',
    backgroundColor: '#ffffff'
};

export const standardTextInputStyles: IStyleFunctionOrObject<ITextFieldStyleProps, ITextFieldStyles> = {
    field: {
        ...baseTextFieldFieldStyles,
        width: '400px'
    }
};

export const largeTextInputStyles: IStyleFunctionOrObject<ITextFieldStyleProps, ITextFieldStyles> = {
    field: {
        ...baseTextFieldFieldStyles,
        width: '380px'
    }
};

export const smallTextInputStyles: IStyleFunctionOrObject<ITextFieldStyleProps, ITextFieldStyles> = {
    field: {
        ...baseTextFieldFieldStyles,
        width: '300px !important'
    }
};

export const textInputWithMarginStyles: IStyleFunctionOrObject<ITextFieldStyleProps, ITextFieldStyles> = {
    root: {
        marginTop: "5px",
    },
    field: {
        ...baseTextFieldFieldStyles,
        width: '400px'
    }
};

export const authInputContainerStyles = {
    height: "5vh",
    width: "20vw",
    marginRight: "5vw",
    marginLeft: "5vw",
    marginTop: "5vh"
};

export const authLabelStyles = {
    fontFamily: "Grotesco",
    fontSize: "20px"
};

export const authLabelSmallStyles = {
    fontFamily: "Grotesco",
    fontSize: "16px"
};

export const authInputContainerWithBottomMarginStyles = {
    ...authInputContainerStyles,
    marginTop: "8vh",
    marginBottom: "12vh"
};
