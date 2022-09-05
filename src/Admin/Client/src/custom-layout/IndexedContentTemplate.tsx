import React from "react";
import { Headline, HeadlineSize, Spacing, Table, TableCell, TableColumn, TableRow } from "@kentico/xperience-admin-components";
import { RoutingContentPlaceholder, usePageCommand } from "@kentico/xperience-admin-base";

interface IndexedContentPageProps {
    readonly pathColumns: TableColumn[];
    readonly pathRows: TableRow[];
    readonly propertyColumns: TableColumn[];
    readonly propertyRows: TableRow[];
}

interface PathDetailArguments
{
    readonly cell: TableCell;
}

const Commands = {
    ShowPathDetail : 'ShowPathDetail'
}

export const IndexedContentTemplate = ({ pathColumns, pathRows, propertyColumns, propertyRows }: IndexedContentPageProps) => {
    const { execute: showPathDetail } = usePageCommand<void, PathDetailArguments>(Commands.ShowPathDetail);

    const pathClicked = (index : number) => {
        // Send cell containing path to back-end
        const row = pathRows[index];
        const cell = row?.cells[0];
        if (cell) {
            showPathDetail({ cell: cell });
        }
    }

    return (
        <RoutingContentPlaceholder>
            <Headline size={HeadlineSize.M}>Indexed content</Headline>
            <Headline size={HeadlineSize.S} spacingTop={Spacing.XL}>Indexed paths</Headline>
            <Table columns={pathColumns} rows={pathRows} onRowClick={pathClicked}/>
            <Headline size={HeadlineSize.S} spacingTop={Spacing.XL}>Indexed properties</Headline>
            <Table columns={propertyColumns} rows={propertyRows} />
        </RoutingContentPlaceholder>
    );
}
