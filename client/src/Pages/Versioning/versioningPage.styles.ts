import { mergeStyles } from "@fluentui/react";

export const titleClassName: string = mergeStyles({
    fontSize: '28px',
    fontWeight: 'bold',
    marginBottom: '16px',
    borderBottom: '3px solid #0078d4',
    paddingBottom: '8px'
});

export const containerClassName: string = mergeStyles({
    minHeight: '100%',
    minWidth: '100%',
    margin: "0px !important",
    padding: "10px"
});

export const buttonClassName: string = mergeStyles({
    width: "125px",
    height: "28px",
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

export const iconClassName: string = mergeStyles({
    marginRight: "10px"
});

export const listContainerClassName: string = mergeStyles({
    marginTop: "50px",
});