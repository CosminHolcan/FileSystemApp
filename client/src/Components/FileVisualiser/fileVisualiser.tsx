import React from "react";
import { IFileVisualiserProps } from "./fileVisualiser.types";

export const FileVisualiser = (props: IFileVisualiserProps): JSX.Element => {
    const extension: string = props.fileName.split('.').pop()?.toLowerCase() as string;

    if (['png', 'jpg', 'jpeg', 'gif', 'bmp', 'svg', 'webp'].includes(extension)) {
        return (
            <div>
                <h3>Image Preview</h3>
                <img src={props.tokenSAS} alt={props.fileName} style={{ maxWidth: '100%', maxHeight: '80vh' }} />
            </div>
        );
    }

    if (extension === 'pdf') {
        return (
            <div>
                <h3>PDF Preview</h3>
                <iframe
                    src={props.tokenSAS}
                    title="PDF Viewer"
                    width="100%"
                    height="800px"
                    style={{ border: 'none' }}
                />
            </div>
        );
    }

    if (['doc', 'docx', 'ppt', 'pptx', 'xls', 'xlsx'].includes(extension)) {
        const encodedUrl = encodeURIComponent(props.tokenSAS);
        const officeViewerUrl = `https://view.officeapps.live.com/op/embed.aspx?src=${encodedUrl}`;

        return (
            <div>
                <h3>Microsoft Document Preview</h3>
                <iframe
                    src={officeViewerUrl}
                    title="Office Document Viewer"
                    width="100%"
                    height="800px"
                    style={{ border: 'none' }}
                />
            </div>
        );
    }

    if (extension === 'txt') {
        return (
            <iframe
                src={props.tokenSAS}
                width="100%"
                height="600px"
                style={{ border: 'none' }}
                title={"version"}
            />
        );
    }

    return (
        <div>
            <h3>Unsupported file type: {extension}</h3>
            <a href={props.tokenSAS} target="_blank" rel="noopener noreferrer">
                Download File
            </a>
        </div>
    );
};