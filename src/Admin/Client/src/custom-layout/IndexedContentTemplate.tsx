import React from 'react'
import { Box, Headline, HeadlineSize, Spacing, Stack, Table, TableCell, TableColumn, TableRow } from '@kentico/xperience-admin-components'
import { RoutingContentPlaceholder, usePageCommand } from '@kentico/xperience-admin-base'
import { useLocalization } from '../localization/LocalizationContext'

interface IndexedContentPageProps {
  readonly pathColumns: TableColumn[]
  readonly pathRows: TableRow[]
  readonly propertyColumns: TableColumn[]
  readonly propertyRows: TableRow[]
}

interface PathDetailArguments {
  readonly cell: TableCell
}

const Commands = {
  ShowPathDetail: 'ShowPathDetail'
}

export const IndexedContentTemplate = ({ pathColumns, pathRows, propertyColumns, propertyRows }: IndexedContentPageProps): JSX.Element => {
  const { localization } = useLocalization();
  const { execute: showPathDetail } = usePageCommand<void, PathDetailArguments>(Commands.ShowPathDetail)

  const pathClicked = (index: number): void => {
    // Send cell containing path to back-end
    const row = pathRows[index]
    const cell = row?.cells[0]
    if (cell !== undefined) {
      showPathDetail({ cell }).catch(() => {})
    }
  }

  return (
        <RoutingContentPlaceholder>
            <Stack spacing={Spacing.XXL}>
                <Headline size={HeadlineSize.M}>{localization.integrations.algolia.content.headlines.main}</Headline>
                <Box>
                    <Headline size={HeadlineSize.S}>{localization.integrations.algolia.content.headlines.paths}</Headline>
                    <Table columns={pathColumns} rows={pathRows} onRowClick={pathClicked}/>
                </Box>
                <Box>
                    <Headline size={HeadlineSize.S}>{localization.integrations.algolia.content.headlines.properties}</Headline>
                    <Table columns={propertyColumns} rows={propertyRows} />
                </Box>
            </Stack>
        </RoutingContentPlaceholder>
  )
}
