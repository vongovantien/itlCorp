export class PagerService {
    getPager(totalItems: number, currentPage: number = 1, pageSize: number = 10, numberPageDisplay: number = 10) {
        // calculate total pages
        const totalPages = Math.ceil(totalItems / pageSize);

        // ensure current page isn't out of range
        if (currentPage < 1) {
            currentPage = 1;
        } else if (currentPage > totalPages) {
            currentPage = totalPages;
        }
        const middle = Math.ceil(numberPageDisplay / 2);
        let startPage: number, endPage: number;
        if (totalPages <= numberPageDisplay) {
            // less than numberPageDisplay so show all
            startPage = 1;
            endPage = totalPages;
        } else {
            // more than numberPageDisplay so calculate start and end pages
            if (currentPage <= (middle + 1)) {
                startPage = 1;
                endPage = numberPageDisplay;
            } else if (currentPage + (middle - 1) >= totalPages) {
                startPage = totalPages - (numberPageDisplay - 1);
                endPage = totalPages;
            } else {
                startPage = currentPage - middle;
                endPage = currentPage + (middle - 1);
            }
        }

        // calculate start and end item indexes
        const startIndex = (currentPage - 1) * pageSize;
        const endIndex = Math.min(startIndex + pageSize - 1, totalItems - 1);

        // create an array of pages to ng-repeat in the pager control
        const pages = Array.from(Array((endPage + 1) - startPage).keys()).map(i => startPage + i);

        return {
            totalItems: totalItems,
            currentPage: currentPage,
            pageSize: pageSize,
            totalPages: totalPages,
            startPage: startPage,
            endPage: endPage,
            startIndex: startIndex,
            endIndex: endIndex,
            pages: pages
        };
    }
}