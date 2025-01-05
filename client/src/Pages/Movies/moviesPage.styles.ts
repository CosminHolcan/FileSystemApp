import { mergeStyles } from "@fluentui/react";

export const buttonContainerClassName: string = mergeStyles({
    marginBottom: "10px",
    marginRight: "20px"
});

export const buttonClassName: string = mergeStyles({
    width: "125px",
    height: "25px",
    backgroundColor: '#0078d4',
    color: 'white',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
    transition: 'background-color 0.3s',
    selectors: {
        '&:hover': {
            backgroundColor: '#005a9e'
        }
    }
});

export const errorMessageClassName: string = mergeStyles({
    fontSize: 17,
    color: "red",
    marginBottom: "15px"
});

export const iconClassName: string = mergeStyles({
    margin: "5px"
});