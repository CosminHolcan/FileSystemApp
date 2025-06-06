import { Icon, Modal, Stack, StackItem, TextField } from "@fluentui/react";
import React from "react";
import { useNavigate, useParams } from "react-router-dom";
import { FileVisualiser } from "../../Components/FileVisualiser/fileVisualiser";
import { useNotification } from "../../Components/Notification/notification";
import { UploadNewContentModal } from "../../Components/UploadNewContentModal/uploadNewContentModal";
import { IAppFile } from "../../Models/AppFile";
import { AppFilesService } from "../../services";
import { downloadBlobWithName, IsNullOrUndefined } from "../../utils";
import { buttonClassName, containerClassName, extenssionClassName, iconClassName, nameStyles, saveButtonClassName, titleClassName } from "./filePage.styles";

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

    const handleSaveNewName = (): void => {
        if (fileId === undefined) {
            return;
        }

        if (name === "") {
            notify("File name can't be empty.");
            return;
        }

        AppFilesService.UpdateFileName(fileId, { jwt: localStorage.getItem("jwt") as string, newFileName: `${name}.${extension}` })
            .then(function (response) {
                notify("Name was succesfully changed.");
            })
            .catch(function (error) {
                notify(error.message);
            });
    }

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
                <button className={buttonClassName} onClick={handleDelete}>
                    <Icon
                        iconName="Delete"
                        className={iconClassName}
                    />
                    Delete
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
                <Stack style={{ marginTop: "10px" }}>
                    <Stack horizontal>
                        <TextField
                            value={name}
                            onChange={(event, newValue) => setName(newValue ?? "")}
                            styles={nameStyles}
                            label="Name"
                        />
                        {!IsNullOrUndefined(file) &&
                            <div className={extenssionClassName}>
                                {"." + extension}
                            </div>
                        }
                        <button className={saveButtonClassName} onClick={handleSaveNewName}>
                            <Icon
                                className={iconClassName}
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