import { Icon, Modal, Stack, StackItem, TextField } from "@fluentui/react";
import React from "react";
import { useNavigate, useParams } from "react-router-dom";
import { FileVisualiser } from "../../Components/FileVisualiser/fileVisualiser";
import { useNotification } from "../../Components/Notification/notification";
import { UploadNewContentModal } from "../../Components/UploadNewContentModal/uploadNewContentModal";
import { IAppFile } from "../../Models/AppFile";
import { AppFilesService } from "../../services";
import { iconWithMarginClassName, pageContainerClassName, pageTitleClassName, primaryButtonClassName, primaryButtonWithMarginClassName, smallTextInputStyles } from "../../styles";
import { downloadBlobWithName, IsNullOrUndefined } from "../../utils";
import { extenssionClassName } from "./filePage.styles";

export const FilePage = (): JSX.Element => {
    const { fileId } = useParams<{ fileId: string }>();
    const navigate = useNavigate();
    const notify = useNotification();

    const [name, setName] = React.useState<string>("");
    const [file, setFile] = React.useState<IAppFile>();
    const [extension, setExtension] = React.useState<string>();
    const [isModalOpen, setIsModalOpen] = React.useState<boolean>(false);

    React.useEffect(() => {
        if (fileId === undefined) {
            return;
        }

        AppFilesService.ReadFileById(fileId, { jwt: localStorage.getItem("jwt") as string })
            .then(function (response) {
                setFile(response.data);
                setInitialFileName(response.data.name);
            })
    }, [fileId]);

    const setInitialFileName = (fileName: string): void => {
        const parts: string[] = fileName.split('.');
        setName(parts.slice(0, -1).join('.'));
        setExtension(parts.pop());
    };

    const handleDownload = (): void => {
        downloadBlobWithName(file?.tokenSAS as string, file?.name as string);
    };

    const handleDelete = (): void => {
        if (fileId === undefined) {
            return;
        }

        if (window.confirm('Are you sure you want to delete this file?')) {
            AppFilesService.DeleteFile(fileId, { jwt: localStorage.getItem("jwt") as string })
                .then(function (response) {
                    navigate("/home");
                });
        }
    };

    const onUploadContent = (newFile: any): void => {
        setFile({ ...file, tokenSAS: newFile.tokenSAS });
        setIsModalOpen(false);
    };

    const onErrorUploadContent = (error: any): void => {
        notify(error.response.data);
        setIsModalOpen(false);
    };

    const handleSaveNewName = (): void => {
        if (fileId === undefined) {
            return;
        }

        if (name === "") {
            notify("File name can't be empty.");
            return;
        }

        const newFileName: string = `${name}.${extension}`;

        AppFilesService.UpdateFileName(fileId, { jwt: localStorage.getItem("jwt") as string, newFileName: newFileName })
            .then(function (response) {
                setFile({ ...file, name: newFileName })
                notify("Name was succesfully changed.");
            })
            .catch(function (error) {
                notify(error.message);
            });
    }

    return (
        <Stack className={pageContainerClassName}>
            {fileId && file &&
                <Modal isOpen={isModalOpen} onDismiss={() => setIsModalOpen(false)}>
                    <UploadNewContentModal
                        onAddedContent={onUploadContent}
                        onErrorAddedContent={onErrorUploadContent}
                        fileId={fileId}
                        fileName={file.name as string}
                        versioning={file.versioning as boolean}
                    />
                </Modal>
            }
            <Stack className={pageTitleClassName} horizontal horizontalAlign="space-between">
                <StackItem style={{ fontSize: "25px", color: "#004e8c" }}>
                    File System App
                </StackItem>
                <StackItem style={{ fontSize: "25px", color: "#004e8c" }}>
                    {localStorage.getItem("userName")}
                </StackItem>
            </Stack>
            <Stack horizontal horizontalAlign="end" tokens={{ childrenGap: 20 }}>
                <button className={primaryButtonClassName} onClick={() => setIsModalOpen(true)}>
                    <Icon
                        iconName="Upload"
                        className={iconWithMarginClassName}
                    />
                    Upload
                </button>
                <button className={primaryButtonClassName} onClick={handleDownload}>
                    <Icon
                        iconName="Download"
                        className={iconWithMarginClassName}
                    />
                    Download
                </button>
                <button className={primaryButtonClassName} onClick={handleDelete}>
                    <Icon
                        iconName="Delete"
                        className={iconWithMarginClassName}
                    />
                    Delete
                </button>
                <button className={primaryButtonClassName} onClick={() => navigate("/home")}>
                    <Icon
                        iconName="Home"
                        className={iconWithMarginClassName}
                    />
                    Home
                </button>
            </Stack>
            {file &&
                <Stack style={{ marginTop: "10px" }}>
                    <Stack horizontal>
                        <TextField
                            value={name}
                            onChange={(event, newValue) => setName(newValue ?? "")}
                            styles={smallTextInputStyles}
                            label="Name"
                        />
                        {!IsNullOrUndefined(file) &&
                            <div className={extenssionClassName}>
                                {"." + extension}
                            </div>
                        }
                        <button className={primaryButtonWithMarginClassName} onClick={handleSaveNewName}>
                            <Icon
                                className={iconWithMarginClassName}
                                iconName="Save"
                            />
                            Save
                        </button>
                    </Stack>
                    <div style={{ marginTop: "10px" }}>
                        <FileVisualiser tokenSAS={file.tokenSAS as string} fileName={file?.name as string} />
                    </div>
                </Stack>
            }
        </Stack>
    );
};