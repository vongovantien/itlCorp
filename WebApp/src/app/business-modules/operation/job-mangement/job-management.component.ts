import { Component, OnInit, ViewChild } from '@angular/core';
import { JobConstants } from 'src/constants/job.const';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { AppList } from 'src/app/app.list';
import { OperationRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { Shipment, CustomDeclaration } from 'src/app/shared/models';
import { SortService } from 'src/app/shared/services';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-job-mangement',
    templateUrl: './job-management.component.html',
})
export class JobManagementComponent extends AppList implements OnInit {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;

    jobStatusConts = {
        overdued: JobConstants.Overdued,
        processing: JobConstants.Processing,
        inSchedule: JobConstants.InSchedule,
        pending: JobConstants.Pending,
        finish: JobConstants.Finish,
        canceled: JobConstants.Canceled,
        warning: JobConstants.Warning
    };

    shipments: Shipment[] = [];
    selectedShipment: Shipment = null;

    totalInProcess = 0;
    totalComplete = 0;
    totalOverdued = 0;
    totalCanceled = 0;

    customClearances: any[] = [];
    deleteMessage: string = '';

    headers: CommonInterface.IHeaderTable[];
    headerCustomClearance: CommonInterface.IHeaderTable[];
    dataSearch: any = {};

    constructor(
        private sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _ngProgressService: NgProgress,
        private _toastService: ToastrService,
        private _operationRepo: OperationRepo
    ) {
        super();
        this.requestSort = this.sortShipment;
        this.requestList = this.getShipments;
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'HBL', field: 'hwbno', sortable: true },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'Service Date', field: 'serviceDate', sortable: true },
            { title: 'Service Port', field: 'polName', sortable: true },
            // { title: 'Flight/Vessel', field: 'flightVessel', sortable: true },
            { title: "Cont Q'ty", field: 'sumContainers', sortable: true },
            { title: "Pack Q'ty", field: 'sumPackages', sortable: true },
            { title: 'G.W', field: 'sumGrossWeight', sortable: true },
            { title: 'CBM', field: 'sumCbm', sortable: true },
            // { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Modified Date', field: 'modifiedDate', sortable: true },
            // { title: 'Status', field: 'currentStatus', sortable: true },
        ];

        this.headerCustomClearance = [
            { title: 'Custom No', field: 'clearanceNo', sortable: true },
            { title: 'Transfer Date', field: 'clearanceDate', sortable: true },
            { title: 'HBl No', field: 'hblid', sortable: true },
            { title: 'Export Country', field: 'exportCountryCode', sortable: true },
            { title: 'import Country', field: 'importCountryCode', sortable: true },
            { title: 'Commodity Code', field: 'commodityCode', sortable: true },
            { title: 'Q\'ty', field: 'qtyCont', sortable: true },
            { title: 'Source', field: 'source', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];
        this.dataSearch.serviceDateFrom = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
        this.dataSearch.serviceDateTo = new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0);
        this.getShipments(this.dataSearch);
    }

    showCustomClearance(jobNo: string, indexsShipment: number) {
        if (this.shipments[indexsShipment].customClearances.length) {
            this.customClearances = this.shipments[indexsShipment].customClearances;
        } else {
            this._progressRef.start();
            this.isLoading = true;
            this._operationRepo.getCustomDeclaration(jobNo)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this._progressRef.complete(); this.isLoading = false; })
                ).subscribe(
                    (res: CustomDeclaration[]) => {
                        if (!!res) {
                            this.customClearances = res.map((item: CustomDeclaration) => new CustomDeclaration(item));
                            this.shipments[indexsShipment].customClearances = this.customClearances;
                        }
                    },
                );
        }
    }

    deleteSipment(shipment: Shipment) {
        this._progressRef.start();
        this._documentRepo.checkShipmentAllowToDelete(shipment.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.selectedShipment = new Shipment(shipment);

                        this.deleteMessage = `Do you want to delete job No ${shipment.jobNo}?`;
                        this.confirmDeleteJobPopup.show();
                    } else {
                        this.canNotDeleteJobPopup.show();
                    }
                },
            );
    }

    onDeleteShipment() {
        this._progressRef.start();
        this._documentRepo.deleteShipment(this.selectedShipment.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.confirmDeleteJobPopup.hide();
                })
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {
                        this._toastService.success(respone.message, 'Delete Success !');
                        this.getShipments();
                    }
                },
            );

    }

    sortShipment() {
        if (!!this.shipments.length) {
            this.shipments = this.sortService.sort(this.shipments, this.sort, this.order);
        }
    }

    sortByCustomClearance(sort: string): void {
        if (!!sort) {
            this.setSortBy(sort, this.sort !== sort ? true : !this.order);
            if (typeof (this.requestSort) === 'function') {
                this.customClearances = this.sortService.sort(this.customClearances, this.sort, this.order);
            }
        }
    }

    sortClassCustomClearance(sort: string): string {
        if (!!sort) {
            let classes = 'sortable ';
            if (this.sort === sort) {
                classes += ('sort-' + (this.order ? 'asc' : 'desc') + ' ');
            }

            return classes;
        }
        return '';
    }

    getShipments(dataSearch?: any) {
        this._progressRef.start();
        this.isLoading = true;
        this._documentRepo.getListShipment(this.page, this.pageSize, dataSearch)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); this.isLoading = false; })
            ).subscribe(
                (responses: any) => {
                    if (!!responses.data) {
                        this.shipments = responses.data.opsTransactions.map((shipment: Shipment) => new Shipment(shipment));

                        this.totalItems = responses.totalItems || 0;
                        this.totalInProcess = responses.data.toTalInProcessing;
                        this.totalOverdued = responses.data.totalOverdued;
                        this.totalComplete = responses.data.toTalFinish;
                        this.totalCanceled = responses.data.totalCanceled;
                    } else {
                        this.totalItems = 0;
                        this.shipments = [];
                    }
                },
            );
    }

    onSearchShipment(dataSearch: any) {
        this.dataSearch = dataSearch;
        this.getShipments(this.dataSearch);
    }

}
