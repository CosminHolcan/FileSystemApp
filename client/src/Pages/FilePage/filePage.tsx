import { Icon, Modal, Stack, StackItem } from "@fluentui/react";
import React from "react";
import { useNavigate, useParams } from "react-router-dom";
import { FileVisualiser } from "../../Components/FileVisualiser/fileVisualiser";
import { UploadNewContentModal } from "../../Components/UploadNewContentModal/uploadNewContentModal";
import { IAppFile } from "../../Models/AppFile";
import { AppFilesService } from "../../services";
import { downloadBlobWithName } from "../../utils";
import { buttonClassName, containerClassName, iconClassName, titleClassName } from "./filePage.styles";

export const FilePage = (): JSX.Element => {
    const { fileId } = useParams<{ fileId: string }>();
    const navigate = useNavigate();

    const [file, setFile] = React.useState<IAppFile>();
    const [isModalOpen, setIsModalOpen] = React.useState<boolean>(false);

    React.useEffect(() => {
        if (fileId === undefined) {
            return;
        }

        AppFilesService.ReadFileById(fileId, { jwt: localStorage.getItem("jwt") as string })
            .then(function (response) {
                setFile(response.data);
            })
    }, [fileId]);

    const handleDownload = (): void => {
        downloadBlobWithName(file?.tokenSAS as string, file?.name as string);
    };

    const onUploadContent = (newFile: any): void => {
        setFile({ ...file, tokenSAS: newFile.tokenSAS });
        setIsModalOpen(false);
    };

    return (
        <Stack className={containerClassName}>
            {fileId && file &&
                <Modal isOpen={isModalOpen} onDismiss={() => setIsModalOpen(false)}>
                    <UploadNewContentModal
                        onAddedContent={onUploadContent}
                        fileId={fileId}
                        fileName={file.name as string}
                        versioning={file.versioning as boolean}
                    />
                </Modal>
            }
            <Stack className={titleClassName} horizontal horizontalAlign="space-between">
                <StackItem style={{ fontSize: "25px", color: "#004e8c" }}>
                    File System App
                </StackItem>
                <StackItem style={{ fontSize: "25px", color: "#004e8c" }}>
                    {localStorage.getItem("userName")}
                </StackItem>
            </Stack>
            <Stack horizontal horizontalAlign="end" tokens={{ childrenGap: 20 }}>
                <button className={buttonClassName} onClick={() => setIsModalOpen(true)}>
                    <Icon
                        iconName="Upload"
                        className={iconClassName}
                    />
                    Upload
                </button>
                <button className={buttonClassName} onClick={handleDownload}>
                    <Icon
                        iconName="Download"
                        className={iconClassName}
                    />
                    Download
                </button>
                <button className={buttonClassName} onClick={() => navigate("/home")}>
                    <Icon
                        iconName="Home"
                        className={iconClassName}
                    />
                    Home
                </button>
            </Stack>
            {file &&
                <div style={{ marginTop: "10px" }}>
                    <FileVisualiser tokenSAS={file.tokenSAS as string} fileName={file?.name as string} />
                </div>
            }
        </Stack>
    );
};