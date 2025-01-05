import { IStyleFunctionOrObject, ITextFieldStyleProps, ITextFieldStyles, mergeStyles } from "@fluentui/react";

export const reviewComponentContainerClassName = mergeStyles({
    border: '1px solid #d1d1d1',
    borderRadius: '8px',
    padding: '16px',
    marginTop: '30px',
    backgroundColor: '#f9f9f9',
    boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
    width: '1000px',
    height: '175px'
});

export const textInputStyles: IStyleFunctionOrObject<ITextFieldStyleProps, ITextFieldStyles> = {
    field: {
        flex: '1',
        padding: '10px',
        border: '1px solid #ccc',
        borderRadius: '4px',
        fontSize: '16px',
        backgroundColor: '#ffffff',
        width: '600px',
        height: '100px'
    }
};