import React from 'react'
import { Box, Headline, HeadlineSize, Spacing, Stack, Table, type TableColumn, type TableRow } from '@kentico/xperience-admin-components'
import { RoutingContentPlaceholder, usePageCommand } from '@kentico/xperience-admin-base'
import localization from '../localization/localization.json'

interface IndexedContentPageProps {
  readonly pathColumns: TableColumn[]
  readonly pathRows: TableRow[]
  readonly propertyColumns: TableColumn[]
  readonly propertyRows: TableRow[]
}

interface PathDetailArguments {
  readonly identifier: string
}

const Commands = {
  ShowPathDetail: 'ShowPathDetail'
}

export const IndexedContentTemplate = ({ pathColumns, pathRows, propertyColumns, propertyRows }: IndexedContentPageProps): JSX.Element => {
   usePageCommand<void, PathDetailArguments>(Commands.ShowPathDetail)

  return (
        <RoutingContentPlaceholder>
            <Stack spacing={Spacing.XXL}>
                <Headline size={HeadlineSize.M}>{localization.integrations.algolia.content.headlines.main}</Headline>
                <Box>
                    <Headline size={HeadlineSize.S}>{localization.integrations.algolia.content.headlines.paths}</Headline>
                </Box>
                <Box>
                    <Headline size={HeadlineSize.S}>{localization.integrations.algolia.content.headlines.properties}</Headline>
                    <Table columns={propertyColumns} rows={propertyRows} />
                </Box>
            </Stack>
        </RoutingContentPlaceholder>
  )
}
