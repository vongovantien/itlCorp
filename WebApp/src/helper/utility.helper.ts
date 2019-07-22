export class UtilityHelper {
    prepareNg2SelectData(dataSource: any[], id: any, text: any) {
        return dataSource.map((item: any) => {
            return { id: item[id], text: item[text] }
        });
    }
}