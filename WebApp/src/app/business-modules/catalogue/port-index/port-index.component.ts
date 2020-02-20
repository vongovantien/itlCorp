import { Component, OnInit, ViewChild } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize, map } from 'rxjs/operators';

import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PORTINDEXCOLUMNSETTING } from './port-index.columns';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { SystemConstants } from 'src/constants/system.const';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { CatalogueRepo, ExportRepo } from 'src/app/shared/repositories';
import { AppList } from 'src/app/app.list';
import { FormPortIndexComponent } from './components/form-port-index.component';

@Component({
    selector: 'app-port-index',
    templateUrl: './port-index.component.html'
})
export class PortIndexComponent extends AppList implements OnInit {
    @ViewChild(FormPortIndexComponent, { static: false }) formPopup: FormPortIndexComponent;
    portIndexSettings: ColumnSetting[] = PORTINDEXCOLUMNSETTING;
    portIndexs: Array<PortIndex>;
    portIndex: PortIndex = new PortIndex();
    criteria: any = { placeType: PlaceTypeEnum.Port };
    addButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.add
    };
    importButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.import
    };
    exportButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.export
    };
    configSearch: any = {
        settingFields: this.portIndexSettings.filter(x => x.allowSearch === true).map(x => ({ "fieldName": x.primaryKey, "displayName": x.header })),
        typeSearch: TypeSearch.outtab
    };
    isDesc: boolean = false;

    constructor(private sortService: SortService,
        private exportRepository: ExportRepo,
        private catalogueRepo: CatalogueRepo,
        private toastService: ToastrService,
        private _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.requestPortIndex;
    }

    ngOnInit() {
        this.requestPortIndex();
        this.getDataCombobox();
    }
    requestPortIndex() {
        this.catalogueRepo.pagingPlace(this.page, this.pageSize, Object.assign({}, this.criteria))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((res: any) => {
                    return {
                        data: res.data,
                        totalItems: res.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.portIndexs = res.data;
                },
            );
    }
    onSearch(event) {
        this.criteria = {
            placeType: PlaceTypeEnum.Port
        };
        if (event.field === "All") {
            this.criteria.all = event.searchString;
        } else {
            const language = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);
            if (language === SystemConstants.LANGUAGES.ENGLISH) {
                if (event.field === "countryName") {
                    this.criteria.countryNameEN = event.searchString;
                }
                if (event.field === "areaName") {
                    this.criteria.areaNameEN = event.searchString;
                }
            } else {
                if (event.field === "countryName") {
                    this.criteria.countryNameVN = event.searchString;
                }
                if (event.field === "areaName") {
                    this.criteria.areaNameVN = event.searchString;
                }
            }
            if (language === SystemConstants.LANGUAGES.VIETNAM) {
            }
            if (event.field === "code") {
                this.criteria.code = event.searchString;
            }
            if (event.field === "nameEn") {
                this.criteria.nameEN = event.searchString;
            }
            if (event.field === "nameVn") {
                this.criteria.nameVN = event.searchString;
            }
            if (event.field === "modeOfTransport") {
                this.criteria.modeOfTransport = event.searchString;
            }
        }
        this.page = 1;
        this.requestList();
    }
    resetSearch(event) {
        this.criteria = {
            placeType: PlaceTypeEnum.Port
        };
        this.onSearch(event);
    }
    showAdd() {
        this.formPopup.portindexForm.reset();
        [this.formPopup.isUpdate, this.formPopup.isSubmitted] = [false, false];
        this.formPopup.title = "Add Port Index";
        this.formPopup.code.enable();
        this.formPopup.show();
    }

    showDetail(item: PortIndex) {
        this.catalogueRepo.getDetailPlace(item.id)
            .pipe(catchError(this.catchError), finalize(() => { }))
            .subscribe(
                (res) => {
                    [this.formPopup.isUpdate, this.formPopup.isSubmitted] = [true, false];
                    this.formPopup.title = "Update Port Index";
                    this.formPopup.code.disable();
                    this.formPopup.portIndex = res;
                    this.formPopup.setFormValue(res);
                    this.formPopup.show();
                }
            );
    }

    getDataCombobox() {
        this.getCountries();
        this.getAreas();
        this.getModeOfTransport();
    }
    getModeOfTransport() {
        this.catalogueRepo.getModeOfTransport()
            .pipe(catchError(this.catchError), finalize(() => { }))
            .subscribe(
                (responses) => {
                    if (responses) {
                        this.formPopup.modes = this.utility.prepareNg2SelectData(responses, 'id', 'name');
                    } else {
                        this.formPopup.modes = [];
                    }
                }
            );
    }
    getAreas() {
        this.catalogueRepo.getAreas()
            .pipe(catchError(this.catchError), finalize(() => { }))
            .subscribe(
                (responses) => {
                    if (responses) {
                        this.formPopup.areas = this.utility.prepareNg2SelectData(responses, 'id', 'nameEn');
                    } else {
                        this.formPopup.areas = [];
                    }
                }
            );
    }
    getCountries() {
        this.catalogueRepo.getCountry()
            .pipe(catchError(this.catchError), finalize(() => { }))
            .subscribe(
                (responses) => {
                    if (responses) {
                        this.formPopup.countries = this.utility.prepareNg2SelectData(responses, 'id', 'nameEn');
                    } else {
                        this.formPopup.countries = [];
                    }
                }
            );
    }

    showConfirmDelete(item) {
        this.portIndex = item;
    }
    onDelete(event) {
        if (event) {
            this.catalogueRepo.deletePlace(this.portIndex.id)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                ).subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this.toastService.success(res.message, '');

                            this.page = 1;
                            this.requestList();
                        } else {
                            this.toastService.error(res.message || 'Có lỗi xảy ra', '');
                        }
                    },
                );

        }
    }

    onSortChange(column) {
        const property = column.primaryKey;
        this.isDesc = !this.isDesc;
        this.portIndexs = this.sortService.sort(this.portIndexs, property, this.isDesc);
    }
    export() {
        this.exportRepository.exportPortIndex(this.criteria)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "PortIndex.xlsx");
                },
            );
    }
}
