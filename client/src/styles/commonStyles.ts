import { mergeStyles } from "@fluentui/react";

export const baseModalContainerStyles = {
    backgroundColor: '#f4f4f4',
    boxShadow: '0px 4px 16px rgba(0, 0, 0, 0.1)',
    borderRadius: '8px',
    padding: '20px',
    maxHeight: '80vh',
    overflowY: 'auto' as const
};

export const smallModalContainerClassName: string = mergeStyles({
    ...baseModalContainerStyles,
    width: '500px',
    height: '250px'
});

export const largeModalContainerClassName: string = mergeStyles({
    ...baseModalContainerStyles,
    width: '850px',
    height: '600px'
});

export const iconWithMarginClassName: string = mergeStyles({
    marginRight: "10px"
});

export const smallMarginLeftClassName: string = mergeStyles({
    marginLeft: "10px"
});

export const smallMarginTopClassName: string = mergeStyles({
    marginTop: "5px"
});

export const mediumMarginTopClassName: string = mergeStyles({
    marginTop: "35px"
});

export const mediumMarginLeftClassName: string = mergeStyles({
    marginLeft: "30px"
});

export const largeMarginTopClassName: string = mergeStyles({
    marginTop: "50px"
});
