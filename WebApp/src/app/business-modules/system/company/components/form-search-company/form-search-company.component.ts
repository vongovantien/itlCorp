import { Component, Output, EventEmitter } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-company',
    templateUrl: './form-search-company.component.html',
})
export class CompanyInformationFormSearchComponent extends AppForm {

    @Output() onSearch: EventEmitter<ISearchCompany> = new EventEmitter<ISearchCompany>();

    selectedType: any = null;
    types: CommonInterface.ICommonTitleValue[];
    configSearch: any;

    constructor() {
        super();
        this.requestSearch = this.searchData;
        this.requestReset = this.onReset;
    }

    ngOnInit(): void {
        this.configSearch = {
            typeSearch: 'outtab',
            settingFields: <CommonInterface.IValueDisplay[]>[
                { displayName: 'Group Code', fieldName: 'code' },
                { displayName: 'Name (EN)', fieldName: 'nameEn' },
                { displayName: 'Name (Local)', fieldName: 'nameVn' },
                { displayName: 'Name Abbr', fieldName: 'shortName' },
                { displayName: 'Department', fieldName: 'active' }
            ]
        };
    }

    searchData(searchObject: ISearchObject) {
        const searchData: ISearchCompany = {
            type: searchObject.field,
            keyword: searchObject.searchString
        };

        this.onSearch.emit(searchData);
        console.log(searchData);
    }

    onReset(data: any) {
        console.log(data);
    }

}

interface ISearchCompany {
    type: string;
    keyword: string;
}

interface ISearchObject extends CommonInterface.IValueDisplay {
    searchString: string;
    field: string;
}
