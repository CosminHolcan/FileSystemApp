import { mergeStyles } from "@fluentui/react";

export const basePrimaryButtonStyles = {
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
};

export const primaryButtonClassName: string = mergeStyles(basePrimaryButtonStyles);

export const primaryButtonWithMarginClassName: string = mergeStyles({
    ...basePrimaryButtonStyles,
    marginTop: "35px",
    marginLeft: "30px"
});

export const largePrimaryButtonStyles = {
    borderRadius: "20px",
    borderWidth: 0,
    width: "10vw",
    height: "4vh",
    backgroundColor: "#0078d4",
    fontFamily: "Grotesco",
    fontSize: 15,
    color: "white",
    cursor: 'pointer',
    transition: 'background-color 0.3s',
    selectors: {
        '&:hover': {
            backgroundColor: '#005a9e'
        }
    }
};

export const largePrimaryButtonHeightVariantStyles = {
    ...largePrimaryButtonStyles,
    height: "6vh"
};
