import { Component, EventEmitter, Output } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-tracking',
    templateUrl: './form-search-tracking.component.html',
    styleUrls: ['./form-search-tracking.component.scss']
})
export class FormSearchTrackingComponent extends AppForm {

    @Output() onChangeKeyWork: EventEmitter<any> = new EventEmitter<any>();
    @Output() onChangeType: EventEmitter<string> = new EventEmitter<string>();
    @Output() onChangeBackGround: EventEmitter<string> = new EventEmitter<string>();
    typeShipment: string = "AIR";
    keySearch: string = '';
    selectedType: any = null;
    types: CommonInterface.ICommonTitleValue[];
    configSearch: any;
    searchObject: any = {
        field: "",
        displayName: "",
        searchString: ""
    };
    defaultSetting: any = { displayName: 'MAWB', fieldName: 'mawb', searchString: "" };
    settingFields: any[] = [this.defaultSetting];
    constructor() {
        super();
    }

    ngOnInit(): void {
        this.configSearch = {
            typeSearch: 'outtab',
            settingFields: <CommonInterface.IValueDisplay[]>[
                { displayName: 'MAWB', fieldName: 'mawb' },
                { displayName: 'HAWB/HBL', fieldName: 'hawb' },
            ]
        };
        this.getSettings(this.configSearch);
    }

    getSettings(configSearch: any): any {
        if (this.configSearch.settingFields) {
            this.configSearch.settingFields.forEach((element: any) => {
                this.settingFields.push(element);
            });
        }
        this.searchObject.field = this.defaultSetting.fieldName;
        this.searchObject.displayName = this.defaultSetting.displayName;
        this.searchObject.searchString = "";
    }

    searchTypeChange(field, event) {
        this.searchObject.displayName = field.displayName;
        this.searchObject.field = field.fieldName;
    }

    onSetTypeShipment(type: string) {
        this.typeShipment = type;
        this.onChangeType.emit(this.typeShipment)
    }

    onShowLoading(value){
        this.onChangeBackGround.emit(value)
    }

    onSearchValue() {
        if(!!this.searchObject.searchString){
            this.onChangeKeyWork.emit({
                [this.searchObject.field]: this.searchObject.searchString,
                shipmentType: this.typeShipment
            })
        }
    }
}
