import { DetailsList, DetailsListLayoutMode, IColumn, Icon, Modal, SelectionMode, Stack, StackItem } from "@fluentui/react";
import React from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { AddFileModal } from "../../Components/AddFileModal/addFileModal";
import { FileLocation } from "../../Enums/FileLocation";
import { Redundancy } from "../../Enums/Redundancy";
import { IAppFile } from "../../Models/AppFile";
import { AppFilesService } from "../../services";
import { getDisplayStringLocation, IsNullOrUndefined } from "../../utils";
import { buttonClassName, containerClassName, iconClassName, listContainerClassName, titleClassName } from "./homePage.styles";

export const HomePage = (): JSX.Element => {
    const navigate = useNavigate();
    const location = useLocation();

    const [isModalOpen, setIsModalOpen] = React.useState<boolean>(false);

    React.useEffect(() => {
        AppFilesService.ReadFilesByUser({ jwt: localStorage.getItem("jwt") as string })
            .then(function (response) {
                setFiles(response.data);
            })
    }, []);

    const [files, setFiles] = React.useState<IAppFile[]>([]);

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
            isResizable: true,
            onRender: item =>
                <Link to={`/file/${item.id}`}>{item.name}</Link>
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
            name: 'Seconday Location',
            fieldName: 'secondaryLocation',
            minWidth: 200,
            isResizable: true,
            onRender: (item: IAppFile) => !IsNullOrUndefined(item.secondaryLocation) ? getDisplayStringLocation(item.secondaryLocation as FileLocation) : ""
        },
        {
            key: 'column4',
            name: 'Redundancy',
            fieldName: 'redundancy',
            minWidth: 200,
            isResizable: true,
            onRender: (item: IAppFile) => (Redundancy[item.redundancy as Redundancy])
        },
        {
            key: 'column5',
            name: 'Supports Versioning',
            fieldName: 'versioning',
            minWidth: 200,
            isResizable: true,
            onRender: item =>
                item.versioning ? (
                    <Link to={`/versioning/${item.id}`}>Yes</Link>
                ) : 'No'
        },
        {
            key: 'column6',
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
                        styles={{ root: { maxHeight: "500px" } }}
                        layoutMode={DetailsListLayoutMode.fixedColumns}
                        selectionPreservedOnEmptyClick={true}
                        selectionMode={SelectionMode.none}
                    />
                </div>
            }
        </Stack>
    )
};