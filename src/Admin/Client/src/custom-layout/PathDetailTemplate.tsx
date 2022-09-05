import { RoutingContentPlaceholder, usePageCommand } from "@kentico/xperience-admin-base";
import { Headline, HeadlineSize, Pagination, Spacing, Table, TableCell, TableColumn, TableRow } from "@kentico/xperience-admin-components";
import React, { useState } from "react";

const ListingCommands = {
    LoadData: 'LoadData'
};

interface PathDetailPageProps
{
    readonly aliasPath: string;
    readonly pageSize: number;
    readonly currentPage: number;
    readonly columns: TableColumn[];
}

interface TemplateParameters {
    currentPage: number,
    pageSize: number
}

interface LoadDataResult {
    readonly rows?: TableRow[];
    readonly totalCount: number;
}

export const PathDetailTemplate = ({aliasPath, columns, pageSize, currentPage}: PathDetailPageProps) => {
    const tableRef = React.createRef<HTMLDivElement>();

    const [tableData, setTableData] = useState<LoadDataResult>({
        rows: undefined,
        totalCount: 0
    });

    const [templateParameters, setTemplateParameters] = useState<TemplateParameters>({
        currentPage: 1,
        pageSize: pageSize
    });

    const { execute: reloadData } = usePageCommand<LoadDataResult, TemplateParameters>(ListingCommands.LoadData, {
        executeOnMount: true,
        after: (result) => {
            if(result) {
                setTableData(result);
            }
        },
        data: templateParameters
    }, [templateParameters]);

    const prepareRows = (rows: TableRow[] | undefined) => {
        if (rows === undefined) {
            return undefined;
        }

        const getCells = (row: TableRow) => {
            const visibleCells: TableCell[] = [];
            row.cells.forEach((cell, index) => {
                if (columns[index].visible) {
                    visibleCells.push(cell);
                }
            });

            return visibleCells;
        };

        return rows.map(row => {
            return {
                identifier: row.identifier,
                disabled: row.disabled,
                cells: getCells(row)
            };
        });
    };

    const totalPages = tableData.totalCount % templateParameters.pageSize > 0 ? Math.trunc(tableData.totalCount / templateParameters.pageSize) + 1 : tableData.totalCount / templateParameters.pageSize;

    const pageHandler = (page: number) => {
        setTemplateParameters(previousParameters => {
            return {
                ...previousParameters,
                currentPage: page
            };
        });
        tableRef.current?.scrollIntoView();
    };
    
    return (
        <RoutingContentPlaceholder>
            <Headline size={HeadlineSize.L}>Path detail</Headline>
            <Headline size={HeadlineSize.S} spacingTop={Spacing.L}>Path</Headline>
            <span>{aliasPath}</span>
            <Headline size={HeadlineSize.S} spacingTop={Spacing.L}>Page types</Headline>
            <Table ref={tableRef} columns={columns.filter(c => c.visible)} rows={prepareRows(tableData.rows)} />
            {tableData.totalCount > templateParameters.pageSize &&
                <div>
                    <div>
                        <Pagination
                            selectedPage={templateParameters.currentPage}
                            totalPages={totalPages}
                            onPageChange={pageHandler} />
                    </div>
                </div>
            }
        </RoutingContentPlaceholder>
    );
}
