import { usePageCommand } from '@kentico/xperience-admin-base'
import { Box, Headline, HeadlineSize, Pagination, Spacing, Stack, Table, TableCell, TableColumn, TableRow } from '@kentico/xperience-admin-components'
import React, { useState } from 'react'

const ListingCommands = {
  LoadData: 'LoadData'
}

interface PathDetailPageProps {
  readonly aliasPath: string
  readonly columns: TableColumn[]
}

interface TemplateParameters {
  currentPage: number
  pageSize: number
}

interface LoadDataResult {
  readonly rows?: TableRow[]
  readonly totalCount: number
}

export const PathDetailTemplate = ({ aliasPath, columns }: PathDetailPageProps): JSX.Element => {
  const tableRef = React.createRef<HTMLDivElement>()

  const [tableData, setTableData] = useState<LoadDataResult>({
    rows: undefined,
    totalCount: 0
  })

  const [templateParameters, setTemplateParameters] = useState<TemplateParameters>({
    currentPage: 1,
    pageSize: 5
  })

  const prepareRows = (rows: TableRow[] | undefined): TableRow[] => {
    if (rows === undefined) {
      return []
    }

    const getCells = (row: TableRow): TableCell[] => {
      const visibleCells: TableCell[] = []
      row.cells.forEach((cell, index) => {
        if (columns[index].visible) {
          visibleCells.push(cell)
        }
      })

      return visibleCells
    }

    return rows.map(row => {
      return {
        identifier: row.identifier,
        disabled: row.disabled,
        cells: getCells(row)
      }
    })
  }

  const totalPages = tableData.totalCount % templateParameters.pageSize > 0 ? Math.trunc(tableData.totalCount / templateParameters.pageSize) + 1 : tableData.totalCount / templateParameters.pageSize

  const pageHandler = (page: number): void => {
    setTemplateParameters(previousParameters => {
      return {
        ...previousParameters,
        currentPage: page
      }
    })
    tableRef.current?.scrollIntoView()
  }

  usePageCommand<LoadDataResult, TemplateParameters>(ListingCommands.LoadData, {
    executeOnMount: true,
    after: (result) => {
      if (result !== undefined) {
        setTableData(result)
      }
    },
    data: templateParameters
  }, [templateParameters])

  return (
        <Stack spacing={Spacing.XL}>
            <Headline size={HeadlineSize.L}>Path detail</Headline>
            <Box>
                <Headline size={HeadlineSize.S}>Path</Headline>
                <span>{aliasPath}</span>
            </Box>
            <Box>
                <Headline size={HeadlineSize.S}>Page types</Headline>
                <Table ref={tableRef} columns={columns.filter(c => c.visible)} rows={prepareRows(tableData.rows)} />
            </Box>
            {tableData.totalCount > templateParameters.pageSize &&
                <Box>
                    <Pagination
                        selectedPage={templateParameters.currentPage}
                        totalPages={totalPages}
                        onPageChange={pageHandler} />
                </Box>
            }
        </Stack>
  )
}
