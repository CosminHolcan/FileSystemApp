import { DetailsList, DetailsListLayoutMode, IColumn, Icon, Modal, Stack, StackItem } from "@fluentui/react";
import React from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { AddFileModal } from "../../Components/CreateMovie/addFileModal";
import { FileLocation } from "../../Enums/FileLocation";
import { Redundancy } from "../../Enums/Redundancy";
import { IAppFile } from "../../Models/AppFile";
import { AppFilesService } from "../../services";
import { getDisplayStringLocation } from "../../utils";
import { buttonClassName, containerClassName, iconClassName, listContainerClassName, titleClassName } from "./homePage.styles";

export const HomePage = (): JSX.Element => {
    const navigate = useNavigate();
    const location = useLocation();

    const [files, setFiles] = React.useState<IAppFile[]>([]);
    const [isModalOpen, setIsModalOpen] = React.useState<boolean>(false);

    React.useEffect(() => {
        AppFilesService.ReadFilesByUser({ jwt: localStorage.getItem("jwt") as string })
            .then(function (response) {
                setFiles(response.data);
            })
    }, []);

    const handleAddFile = (newFile: IAppFile): void => {
        setFiles([...files, newFile]);
        setIsModalOpen(false);
    };

    const handleLogout = (): void => {
        localStorage.removeItem("jwt");
        localStorage.removeItem("userName");
        navigate("/login");
    };

    const columns: IColumn[] = [
        {
            key: 'column1',
            name: 'File name',
            fieldName: 'name',
            minWidth: 200,
            isResizable: true
        },
        {
            key: 'column2',
            name: 'Primary Location',
            fieldName: 'location',
            minWidth: 200,
            isResizable: true,
            onRender: (item: IAppFile) => getDisplayStringLocation(item.location as FileLocation)
        },
        {
            key: 'column3',
            name: 'Redundancy',
            fieldName: 'redundancy',
            minWidth: 200,
            isResizable: true,
            onRender: (item: IAppFile) => (Redundancy[item.redundancy as Redundancy])
        },
        {
            key: 'column4',
            name: 'Supports Versioning',
            fieldName: 'versioning',
            minWidth: 200,
            isResizable: true,
            onRender: item => (item.hasVersioning ? 'Yes' : 'No'),
        },
        {
            key: 'column5',
            name: 'Creation Date',
            fieldName: 'creationDate',
            minWidth: 200,
            isResizable: true,
        },
    ];


    return (
        <Stack className={containerClassName}>
            <Modal isOpen={isModalOpen} onDismiss={() => setIsModalOpen(false)}>
                <AddFileModal
                    onAddedFile={handleAddFile}
                />
            </Modal>
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
                    Add File
                </button>
                <button className={buttonClassName} onClick={handleLogout}>
                    <Icon
                        iconName="SignOut"
                        className={iconClassName}
                    />
                    Logout
                </button>
            </Stack>
            {files?.length > 0 &&
                <div className={listContainerClassName}>
                    <DetailsList
                        items={files}
                        columns={columns}
                        setKey="set"
                        layoutMode={DetailsListLayoutMode.fixedColumns}
                        selectionPreservedOnEmptyClick={true}
                        ariaLabelForSelectionColumn="Toggle selection"
                        ariaLabelForSelectAllCheckbox="Toggle selection for all items"
                    />
                </div>
            }
        </Stack>
    )
};