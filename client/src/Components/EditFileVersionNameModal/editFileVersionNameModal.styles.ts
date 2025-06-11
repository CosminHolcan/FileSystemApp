import { IStyleFunctionOrObject, ITextFieldStyleProps, ITextFieldStyles, mergeStyles } from "@fluentui/react";

export const modalContainerClassName: string = mergeStyles({
    backgroundColor: '#f4f4f4',
    boxShadow: '0px 4px 16px rgba(0, 0, 0, 0.1)',
    borderRadius: '8px',
    padding: '20px',
    width: '500px',
    height: '250px',
    maxHeight: '80vh',
    overflowY: 'auto'
});

export const nameStyles: IStyleFunctionOrObject<ITextFieldStyleProps, ITextFieldStyles> = {
    field: {
        flex: '1',
        padding: '10px',
        border: '1px solid #ccc',
        borderRadius: '4px',
        fontSize: '16px',
        backgroundColor: '#ffffff',
        width: '400px'
    }
};

export const errorMessageClassName: string = mergeStyles({
    fontSize: 17,
    color: "red",
    marginBottom: "15px"
});
