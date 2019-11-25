import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, map, finalize } from 'rxjs/operators';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Params } from '@angular/router';
import { SortService } from 'src/app/shared/services';
import { CdNoteAddPopupComponent } from 'src/app/business-modules/documentation/sea-fcl-import/components/popup/add-cd-note/add-cd-note.popup';
import { CdNoteDetailPopupComponent } from 'src/app/business-modules/documentation/sea-fcl-import/components/popup/detail-cd-note/detail-cd-note.popup';

@Component({
    selector: 'cd-note-list',
    templateUrl: './cd-note-list.component.html',
})
export class CdNoteListComponent extends AppList {
    @ViewChild(CdNoteAddPopupComponent, { static: false }) cdNoteAddPopupComponent: CdNoteAddPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteCdNotePopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteCdNotePopup: InfoPopupComponent;
    @ViewChild(CdNoteDetailPopupComponent, { static: false }) cdNoteDetailPopupComponent: CdNoteDetailPopupComponent;

    headers: CommonInterface.IHeaderTable[];
    idMasterBill: string = '';
    cdNoteGroups: any[] = [];
    initGroup: any[] = [];
    deleteMessage: string = '';
    selectedCdNoteId: string = '';

    isDesc = true;
    sortKey: string = '';

    constructor(
        private _documentationRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _progressService: NgProgress,
        private _activedRoute: ActivatedRoute,
        private _sortService: SortService,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit(): void {
        this._activedRoute.params.subscribe((param: Params) => {
            console.log('alakd', param.id)
            //if (param.id) {
                //console.log(param.id);
                this.idMasterBill = "9f479944-8b9d-4c23-aa03-07252a372c05";//param.id;
                this.getListCdNote(this.idMasterBill);
            //}
        });
        this.headers = [
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Note No', field: 'code', sortable: true },
            { title: 'Charges Count', field: 'total_charge', sortable: true,  },
            { title: 'Balance Amount', field: 'total', sortable: true, width: 220},
            { title: 'Creator', field: 'userCreated', sortable: true },
            { title: 'Create Date', field: 'datetimeCreated', sortable: true },
            { title: 'SOA', field: 'soaNo', sortable: true },
        ];
    }

    ngAfterViewInit() {
        this.cdNoteAddPopupComponent.getListSubjectPartner(this.idMasterBill);
        this.cdNoteDetailPopupComponent.cdNoteEditPopupComponent.getListSubjectPartner(this.idMasterBill);
    }

    openPopupAdd() {
        this.cdNoteAddPopupComponent.action = 'create';
        this.cdNoteAddPopupComponent.currentMBLId = this.idMasterBill;
        this.cdNoteAddPopupComponent.show();
    }

    openPopupDetail(jobId: string, cdNote: string) {
        this.cdNoteDetailPopupComponent.jobId = jobId;
        this.cdNoteDetailPopupComponent.cdNote = cdNote;
        this.cdNoteDetailPopupComponent.getDetailCdNote(jobId, cdNote);
        this.cdNoteDetailPopupComponent.show();
    }

    getListCdNote(id: string) {
        this.isLoading = true;
        this._documentationRepo.getListCdNoteByMasterBill(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.cdNoteGroups = res;
                    this.initGroup = res;
                    console.log(this.cdNoteGroups)
                },
            );
    }

    checkDeleteCdNote(id: string) {
        this._progressRef.start();
        this._documentationRepo.checkCdNoteAllowToDelete(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.selectedCdNoteId = id;
                        console.log(this.selectedCdNoteId)
                        this.deleteMessage = `All related information will be lost? Are you sure you want to delete this Credit/Debit Note?`;
                        this.confirmDeleteCdNotePopup.show();
                    } else {
                        this.canNotDeleteCdNotePopup.show();
                    }
                },
            );
    }

    onDeleteCdNote() {
        console.log(this.selectedCdNoteId)
        this._progressRef.start();
        this._documentationRepo.deleteCdNote(this.selectedCdNoteId)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.confirmDeleteCdNotePopup.hide();
                })
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {
                        this._toastService.success(respone.message, 'Delete Success !');
                        this.getListCdNote(this.idMasterBill);
                    }
                },
            );
    }

    sortCdNotes(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.cdNoteGroups.forEach(element => {
            element.listCDNote = this._sortService.sort(element.listCDNote, property, this.isDesc);
        });
    }

    onRequestCdNoteChange(dataRequest: any) {
        this.getListCdNote(this.idMasterBill);
    }

    onDeletedCdNote(data: any) {
        this.getListCdNote(this.idMasterBill);
    }

    //Charge keyword search
    onChangeKeyWord(keyword: string) {
        this.cdNoteGroups = this.initGroup;
        //TODO improve search.
        if (!!keyword) {
            keyword = keyword.toLowerCase();
            // Search group
            let dataGrp = this.cdNoteGroups.filter((item: any) => item.partnerNameEn.toLowerCase().toString().search(keyword) !== -1)
            // Không tìm thấy group thì search tiếp list con của group
            if (dataGrp.length == 0) {
                let arrayCharge = [];
                for (const group of this.cdNoteGroups) {
                    const data = group.listCDNote.filter((item: any) => item.type.toLowerCase().toString().search(keyword) !== -1 || item.code.toLowerCase().toString().search(keyword) !== -1)
                    if (data.length > 0) {
                        arrayCharge.push({ id: group.id, partnerNameEn: group.partnerNameEn, partnerNameVn: group.partnerNameVn, listCDNote: data });
                    }
                }
                dataGrp = arrayCharge;
            }
            return this.cdNoteGroups = dataGrp;
        } else {
            this.cdNoteGroups = this.initGroup;
        }
    }
}
