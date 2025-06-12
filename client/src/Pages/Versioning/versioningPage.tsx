import { DetailsList, DetailsListLayoutMode, Dropdown, IColumn, Icon, IconButton, IDropdownOption, Modal, SelectionMode, Stack, StackItem, Toggle } from "@fluentui/react";
import React from "react";
import { useNavigate, useParams } from "react-router-dom";
import { AddVersionModal } from "../../Components/AddVersionModal/addVersionModal";
import { EditFileVersionNameModal } from "../../Components/EditFileVersionNameModal/editFileVersionNameModal";
import { FileVisualiser } from "../../Components/FileVisualiser/fileVisualiser";
import { useNotification } from "../../Components/Notification/notification";
import { IFileVersion } from "../../Models/FileVersion";
import { IFileWithVersions } from "../../Models/FileWithVersions";
import { AppFilesService, FileVersionsService } from "../../services";
import { downloadBlobWithName } from "../../utils";
import { buttonClassName, containerClassName, iconClassName, listContainerClassName, titleClassName } from "./versioningPage.styles";

export const VersioningPage = (): JSX.Element => {
    const { fileId } = useParams<{ fileId: string }>();
    const navigate = useNavigate();
    const notify = useNotification();

    const [file, setFile] = React.useState<IFileWithVersions>();
    const [fileVersions, setFileVersions] = React.useState<IFileVersion[]>([]);
    const [isAddVersionModalOpen, setIsAddVersionModalOpen] = React.useState<boolean>(false);
    const [compareVersions, setCompareVersions] = React.useState<boolean>(false);
    const [firstSelectedFile, setFirstSelectedFile] = React.useState<string>();
    const [secondSelectedFile, setSecondSelectedFile] = React.useState<string>();
    const [editingFileVersion, setEditingFileVersion] = React.useState<IFileVersion>();
    const [isEditNameModalOpen, setIsEditNameModalOpen] = React.useState<boolean>(false);

    React.useEffect(() => {
        if (fileId === undefined) {
            return;
        }

        AppFilesService.ReadFileWithVersionsById(fileId, { jwt: localStorage.getItem("jwt") as string })
            .then(function (response) {
                setFileVersions(response.data.versions);
                setFile(response.data);
            })
    }, [fileId]);

    const handleDownload = (fileVersion: IFileVersion): void => {
        downloadBlobWithName(fileVersion.tokenSAS as string, getDownloadName(fileVersion));
    };

    const handleEdit = (fileVersion: IFileVersion): void => {
        setEditingFileVersion(fileVersion);
        setIsEditNameModalOpen(true);
    };

    const onModifyName = (fileVersion: IFileVersion, newName: string): void => {
        const fileVersionIndex: number = fileVersions.findIndex((fv: IFileVersion) => fv.id === fileVersion.id);
        if (fileVersionIndex === -1) {
            setIsEditNameModalOpen(false);
            setEditingFileVersion(undefined);
            return;
        }

        const newFileVersions: IFileVersion[] = [...fileVersions];
        newFileVersions[fileVersionIndex].name = newName;
        setFileVersions(newFileVersions);
        setIsEditNameModalOpen(false);
    };

    const handleDelete = (fileVersion: IFileVersion): void => {
        FileVersionsService.DeleteFileVersion(fileVersion.id as string, { jwt: localStorage.getItem("jwt") as string })
            .then(function (response) {
                const newFileVersions: IFileVersion[] = fileVersions.filter(fv => fv.id !== fileVersion.id)
                setFileVersions(newFileVersions);
                if (firstSelectedFile === fileVersion.id) {
                    setFirstSelectedFile(undefined);
                }
                if (secondSelectedFile === fileVersion.id) {
                    setSecondSelectedFile(undefined);
                }            
            })
            .catch(function (error) {
                notify(error.response.data)
            });
    };

    const getDownloadName = (fileVersion: IFileVersion): string => {
        const lastDotIndex: number = file?.name.lastIndexOf('.') as number;
        const fileName: string = (lastDotIndex !== -1 ? file?.name.substring(0, lastDotIndex) : file?.name) as string;
        const extension: string = (lastDotIndex !== -1 ? file?.name.substring(lastDotIndex + 1) : '') as string;

        return `${fileName}_${fileVersion.name}.${extension}`;
    };

    const columns: IColumn[] = [
        {
            key: 'column1',
            name: 'Version Name',
            fieldName: 'name',
            minWidth: 200,
            isResizable: true
        },
        {
            key: 'column2',
            name: 'Time Created',
            fieldName: 'creationTime',
            minWidth: 200,
            isResizable: true,
        },
        {
            key: 'column3',
            name: 'Download',
            fieldName: 'download',
            minWidth: 200,
            isResizable: true,
            onRender: (item: IFileVersion) =>
                <IconButton
                    iconProps={{ iconName: 'Download' }}
                    title="Download"
                    ariaLabel="Download"
                    onClick={() => handleDownload(item)}
                />
        },
        {
            key: 'column4',
            name: 'Edit Name',
            fieldName: 'edit',
            minWidth: 200,
            isResizable: true,
            onRender: (item: IFileVersion) =>
                <IconButton
                    iconProps={{ iconName: 'Edit' }}
                    title="Edit"
                    ariaLabel="Edit"
                    onClick={() => handleEdit(item)}
                />
        },
        {
            key: 'column5',
            name: 'Delete',
            fieldName: 'delete',
            minWidth: 200,
            isResizable: true,
            onRender: (item: IFileVersion) =>
                item.id !== fileVersions.at(-1)?.id &&
                <IconButton
                    iconProps={{ iconName: 'Delete' }}
                    title="Delete"
                    ariaLabel="Delete"
                    onClick={() => handleDelete(item)}
                />
        }
    ];

    const addNewVersion = (fileVersion: IFileVersion): void => {
        setFileVersions([...fileVersions, fileVersion]);
        setIsAddVersionModalOpen(false);
    };

    const onErrorHandler = (error: any): void => {
        notify(error.response.data);
        setIsAddVersionModalOpen(false);
    };

    const onToggleChange = (event: React.MouseEvent<HTMLElement>, checked?: boolean) => {
        const newValue: boolean = checked ?? false;
        setCompareVersions(checked ?? false);
        if (!newValue) {
            setSecondSelectedFile(undefined);
        }
    };

    const toDropdownOptions = (excludeId?: string): IDropdownOption[] =>
        fileVersions
            .filter(fv => fv.id !== excludeId)
            .map(file => ({
                key: file.id as string,
                text: file.name as string,
            }));

    const findFileVersionById = (fileVersionId: string): IFileVersion => {
        return fileVersions.find((fileVersion: IFileVersion) => fileVersion.id === fileVersionId) as IFileVersion;
    };

    return (
        <Stack className={containerClassName}>
            {fileId && file &&
                <Modal isOpen={isAddVersionModalOpen} onDismiss={() => setIsAddVersionModalOpen(false)}>
                    <AddVersionModal
                        onAddedVersion={addNewVersion}
                        onErrorAddVersion={onErrorHandler}
                        originalFileId={fileId}
                        originalFileName={file.name}
                    />
                </Modal>
            }
            {editingFileVersion &&
                <Modal isOpen={isEditNameModalOpen} onDismiss={() => { setIsEditNameModalOpen(false); setEditingFileVersion(undefined); }}>
                    <EditFileVersionNameModal
                        fileVersion={editingFileVersion}
                        onModifiedName={onModifyName}
                        onError={onErrorHandler}
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
                <button className={buttonClassName} onClick={() => setIsAddVersionModalOpen(true)}>
                    <Icon
                        iconName="Add"
                        className={iconClassName}
                    />
                    Add version
                </button>
                <button className={buttonClassName} onClick={() => navigate("/home")}>
                    <Icon
                        iconName="Home"
                        className={iconClassName}
                    />
                    Home
                </button>
            </Stack>
            {fileVersions?.length > 0 &&
                <div className={listContainerClassName}>
                    <DetailsList
                        items={fileVersions}
                        columns={columns}
                        setKey="set"
                        layoutMode={DetailsListLayoutMode.fixedColumns}
                        selectionPreservedOnEmptyClick={true}
                        styles={{ root: { maxHeight: "300px" } }}
                        ariaLabelForSelectionColumn="Toggle selection"
                        ariaLabelForSelectAllCheckbox="Toggle selection for all items"
                        selectionMode={SelectionMode.none}
                    />
                </div>
            }
            {fileVersions?.length === 1 &&
                <div style={{ marginTop: "50px" }}>
                    <FileVisualiser tokenSAS={fileVersions[0].tokenSAS as string} fileName={file?.name as string} />
                </div>
            }
            {fileVersions?.length > 1 &&
                <div>
                    <Stack horizontalAlign="end" style={{ width: "96vw", marginTop: "30px", marginBottom: "30px" }}>
                        <Toggle
                            label="Compare versions"
                            inlineLabel
                            checked={compareVersions}
                            onChange={onToggleChange}
                            onText="On"
                            offText="Off"
                        />
                    </Stack>
                    {compareVersions
                        ?
                        <Stack horizontal style={{ width: "95vw" }} tokens={{ childrenGap: 50 }}>
                            <StackItem grow>
                                <Dropdown
                                    label="First Version"
                                    options={toDropdownOptions(secondSelectedFile)}
                                    selectedKey={firstSelectedFile}
                                    onChange={(event, option) => setFirstSelectedFile(option?.key as string)}
                                    placeholder="Select first version"
                                    styles={{ dropdown: { width: 300 } }}
                                />
                                {firstSelectedFile &&
                                    <div style={{ marginTop: "10px" }}>
                                        <FileVisualiser tokenSAS={findFileVersionById(firstSelectedFile).tokenSAS as string} fileName={file?.name as string} />
                                    </div>
                                }
                            </StackItem>
                            <StackItem grow>
                                <Dropdown
                                    label="Second Version"
                                    options={toDropdownOptions(firstSelectedFile)}
                                    selectedKey={secondSelectedFile}
                                    onChange={(event, option) => setSecondSelectedFile(option?.key as string)}
                                    placeholder="Select second version"
                                    styles={{ dropdown: { width: 300 } }}
                                />
                                {secondSelectedFile &&
                                    <div style={{ marginTop: "10px" }}>
                                        <FileVisualiser tokenSAS={findFileVersionById(secondSelectedFile).tokenSAS as string} fileName={file?.name as string} />
                                    </div>
                                }
                            </StackItem>
                        </Stack>
                        : <Stack>
                            <Dropdown
                                label="Version"
                                options={toDropdownOptions(secondSelectedFile)}
                                selectedKey={firstSelectedFile}
                                onChange={(event, option) => setFirstSelectedFile(option?.key as string)}
                                placeholder="Select version"
                                styles={{ dropdown: { width: 300 } }}
                            />
                            {firstSelectedFile &&
                                <div style={{ marginTop: "10px" }}>
                                    <FileVisualiser tokenSAS={findFileVersionById(firstSelectedFile).tokenSAS as string} fileName={file?.name as string} />
                                </div>
                            }
                        </Stack>
                    }
                </div>
            }
        </Stack>
    );
};