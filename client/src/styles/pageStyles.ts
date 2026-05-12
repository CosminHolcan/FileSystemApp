import { mergeStyles } from "@fluentui/react";

export const pageTitleClassName: string = mergeStyles({
    fontSize: '28px',
    fontWeight: 'bold',
    marginBottom: '16px',
    borderBottom: '3px solid #0078d4',
    paddingBottom: '8px'
});

export const pageContainerClassName: string = mergeStyles({
    minHeight: '100%',
    minWidth: '100%',
    margin: "0px !important",
    padding: "10px"
});

export const pageListContainerClassName: string = mergeStyles({
    marginTop: "50px"
});
