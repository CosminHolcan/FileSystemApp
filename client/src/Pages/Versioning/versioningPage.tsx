import { DetailsList, DetailsListLayoutMode, Dropdown, IColumn, Icon, IconButton, IDropdownOption, Modal, SelectionMode, Stack, StackItem, Toggle } from "@fluentui/react";
import React from "react";
import { useNavigate, useParams } from "react-router-dom";
import { AddVersionModal } from "../../Components/AddVersionModal/addVersionModal";
import { FileVisualiser } from "../../Components/FileVisualiser/fileVisualiser";
import { IFileVersion } from "../../Models/FileVersion";
import { IFileWithVersions } from "../../Models/FileWithVersions";
import { AppFilesService } from "../../services";
import { downloadBlobWithName } from "../../utils";
import { buttonClassName, containerClassName, iconClassName, listContainerClassName, titleClassName } from "./versioningPage.styles";

export const VersioningPage = (): JSX.Element => {
    const { fileId } = useParams<{ fileId: string }>();
    const navigate = useNavigate();

    const [file, setFile] = React.useState<IFileWithVersions>();
    const [fileVersions, setFileVersions] = React.useState<IFileVersion[]>([]);
    const [isModalOpen, setIsModalOpen] = React.useState<boolean>(false);
    const [compareVersions, setCompareVersions] = React.useState<boolean>(false);
    const [firstSelectedFile, setFirstSelectedFile] = React.useState<string>();
    const [secondSelectedFile, setSecondSelectedFile] = React.useState<string>();

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
        }
    ];

    const addNewVersion = (fileVersion: IFileVersion): void => {
        setFileVersions([...fileVersions, fileVersion]);
        setIsModalOpen(false);
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
                <Modal isOpen={isModalOpen} onDismiss={() => setIsModalOpen(false)}>
                    <AddVersionModal
                        onAddedVersion={addNewVersion}
                        originalFileId={fileId}
                        originalFileName={file.name}
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
                                    <FileVisualiser tokenSAS={findFileVersionById(firstSelectedFile).tokenSAS as string} fileName={file?.name as string} />
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
                                    <FileVisualiser tokenSAS={findFileVersionById(secondSelectedFile).tokenSAS as string} fileName={file?.name as string} />
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
                                <FileVisualiser tokenSAS={findFileVersionById(firstSelectedFile).tokenSAS as string} fileName={file?.name as string} />
                            }
                        </Stack>
                    }
                </div>
            }
        </Stack>
    );
};