import { InitProfitHBLAction, GetDetailHBLAction, InitListHBLAction } from './../../store/actions/hbl.action';
import { AppList } from "src/app/app.list";
import { SortService } from "@services";
import { HouseBill, CsTransactionDetail, CsTransaction, Crystal } from "@models";
import { getHBLSState, IShareBussinessState, GetContainersHBLAction, GetProfitHBLAction, GetBuyingSurchargeAction, GetSellingSurchargeAction, GetOBHSurchargeAction, GetListHBLAction, TransactionGetDetailAction, getTransactionLocked, getHBLLoadingState, getSurchargeLoadingState, getTransactionDetailCsTransactionState, GetContainerAction, LoadListPartnerForKeyInSurcharge } from "../../store";
import { Store } from "@ngrx/store";
import { ActivatedRoute, Router } from "@angular/router";
import { NgxSpinnerService } from "ngx-spinner";
import { ViewChild, Directive } from "@angular/core";
import { ConfirmPopupComponent, Permission403PopupComponent, InfoPopupComponent, ReportPreviewComponent } from "@common";
import { ToastrService } from "ngx-toastr";
import { CatalogueRepo, DocumentationRepo, ExportRepo, SystemFileManageRepo } from "@repositories";
import { ICrystalReport } from "@interfaces";

import { takeUntil, catchError, map, switchMap, concatMap, mergeMap } from "rxjs/operators";
import isUUID from 'validator/es/lib/isUUID';
import { delayTime } from "@decorators";
import { combineLatest, of } from 'rxjs';
import { RoutingConstants, SystemConstants } from '@constants';
import { ShareBussinessMassUpdatePodComponent } from '@share-bussiness';
import { HttpResponse } from '@angular/common/http';
import { InjectViewContainerRefDirective } from '@directives';


@Directive()
export abstract class AppShareHBLBase extends AppList implements ICrystalReport {
    @ViewChild(ShareBussinessMassUpdatePodComponent) massUpdatePODComponent: ShareBussinessMassUpdatePodComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;

    houseBills: HouseBill[] = [];

    totalCBM: number;
    totalGW: number;
    totalCW: number;
    totalQty: number;

    selectedHbl: CsTransactionDetail;
    selectedTabSurcharge: string = 'BUY';
    selectedIndexHBL: number = -1;
    selectedHblId: string = null;

    shipmentDetail: CsTransaction;

    jobId: string = '';
    spinnerSurcharge: string = 'spinnerSurcharge';

    serviceType: CommonType.SERVICE_TYPE = 'sea';
    transactionType: string;

    constructor(
        protected _sortService: SortService,
        protected _store: Store<IShareBussinessState>,
        protected _spinner: NgxSpinnerService,
        protected _toastService: ToastrService,
        protected _documentRepo: DocumentationRepo,
        protected _activedRoute: ActivatedRoute,
        protected _router: Router,
        protected _catalogueRepo: CatalogueRepo,
        protected _exportRepo: ExportRepo,
        protected _fileMngtRepo: SystemFileManageRepo

    ) {
        super();
        this.requestSort = this.sortLocal;
    }


    ngOnInit() {
        this.subscription = combineLatest([
            this._activedRoute.params,
            this._activedRoute.queryParams,
            this._activedRoute.data
        ]).pipe(
            map(([params, qParams, qData]) => ({ ...params, ...qParams, ...qData })),
            takeUntil(this.ngUnsubscribe)
        ).subscribe(
            (param: any) => {
                if (param.jobId && isUUID(param.jobId)) {
                    this.jobId = param.jobId;
                    if (param.selected) {
                        this.selectedHblId = param.selected;
                    }
                    if (param.serviceId) {
                        this.transactionType = param.serviceId;
                    }
                    this._store.dispatch(new InitListHBLAction({ jobId: this.jobId }));
                    this._store.dispatch(new GetListHBLAction({ jobId: this.jobId }));
                    this._store.dispatch(new TransactionGetDetailAction(this.jobId));
                    this.getDetailShipment();
                    this.getHouseBills(this.jobId);
                } else {
                    this.gotoList();
                }
            });

        this.configHBL();

        this.isLocked = this._store.select(getTransactionLocked);
        this.isLoading = this._store.select(getHBLLoadingState);

        this.listenShortcutMovingTab();
    }


    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    configHBL() {
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true, width: 100 },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'Salesman', field: 'saleManName', sortable: true },
            { title: 'Departure', field: 'finalDestinationPlace', sortable: true },
            { title: 'Destination', field: 'finalDestinationPlace', sortable: true },
            { title: 'Package', field: 'packages', sortable: true },
            { title: 'C.W', field: 'cw', sortable: true },
            { title: 'G.W', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Group', field: 'group', sortable: true },
            { title: 'Department', field: 'department', sortable: true }
        ];
    }

    sortLocal(sort: string): void {
        this.houseBills = this._sortService.sort(this.houseBills, sort, this.order);
    }

    getHouseBills(id: string) {
        this._store.select(getHBLSState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (hbls: any[]) => {
                    this.houseBills = hbls || [];
                    if (!!this.houseBills.length) {
                        this.totalGW = this.houseBills.reduce((acc: number, curr: HouseBill) => acc += curr.gw, 0);
                        this.totalCBM = this.houseBills.reduce((acc: number, curr: HouseBill) => acc += curr.cbm, 0);
                        this.totalCW = this.houseBills.reduce((acc: number, curr: HouseBill) => acc += curr.cw, 0);
                        this.totalQty = this.houseBills.reduce((acc: number, curr: HouseBill) => acc += curr.packageQty, 0);

                        if (this.selectedHblId) {
                            if (!this.houseBills.some((house: HouseBill) => house.id === this.selectedHblId)) {
                                this._toastService.error('This House Bill does not exist!');
                            }
                            const currenthbl = this.houseBills.find((house: HouseBill) => house.id === this.selectedHblId);
                            if (currenthbl) {
                                this.selectHBL(currenthbl);
                            }
                        } else {
                            this.selectHBL(this.houseBills[0]);
                        }
                    } else {
                        this.selectedHbl = null;

                        this._store.dispatch(new InitProfitHBLAction())
                    }
                }
            );
    }

    getDetailShipment() {
        this._store.select<any>(getTransactionDetailCsTransactionState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.shipmentDetail = res;
                        // console.log('shipmentDetail', this.shipmentDetail)
                    }
                },
            );
    }

    selectHBL(hbl: HouseBill) {
        if (!this.selectedHbl || !!this.selectedHbl && this.selectedHbl.id !== hbl.id) {
            this.selectedHbl = new CsTransactionDetail(hbl);

            // * Get container, Job detail, Surcharge with hbl id, JobId.
            //this._store.dispatch(new GetDetailHBLSuccessAction(hbl));
            this._store.dispatch(new GetDetailHBLAction(hbl.id));
            this._store.dispatch(new GetContainersHBLAction({ hblid: hbl.id }));
            if (this.serviceType === 'sea') {
                this._store.dispatch(new GetContainerAction({ mblid: this.jobId }));
            }
            this._store.dispatch(new GetProfitHBLAction(this.selectedHbl.id));
            switch (this.selectedTabSurcharge) {
                case 'BUY':
                    this._store.dispatch(new GetBuyingSurchargeAction({ type: 'BUY', hblId: this.selectedHbl.id }));
                    break;
                case 'SELL':
                    this._catalogueRepo.getAgreement(
                        { partnerId: this.selectedHbl.customerId, status: true, salesmanId: this.selectedHbl.saleManId, service: this.selectedHbl.transactionType }
                    )
                        .subscribe(
                            (res) => {
                                this._store.dispatch(LoadListPartnerForKeyInSurcharge(
                                    {
                                        office: (this.selectedHbl as any)?.officeId,
                                        salemanId: (this.selectedHbl as any).saleManId,
                                        service: this.selectedHbl.transactionType,
                                        contractType: res[0]?.contractType
                                    })
                                );
                            }
                        )

                    this._store.dispatch(new GetSellingSurchargeAction({ type: 'SELL', hblId: this.selectedHbl.id }));
                    break;
                case 'OBH':
                    this._catalogueRepo.getAgreement(
                        { partnerId: this.selectedHbl.customerId, status: true, salesmanId: this.selectedHbl.saleManId, service: this.selectedHbl.transactionType }
                    )
                        .subscribe(
                            (res) => {
                                this._store.dispatch(LoadListPartnerForKeyInSurcharge(
                                    {
                                        office: (this.selectedHbl as any)?.officeId,
                                        salemanId: (this.selectedHbl as any).saleManId,
                                        service: this.selectedHbl.transactionType,
                                        contractType: res[0]?.contractType
                                    })
                                );
                            }
                        )
                    this._store.dispatch(new GetOBHSurchargeAction({ type: 'OBH', hblId: this.selectedHbl.id }));
                    break;
                default:
                    break;
            }
        }
    }

    showDeletePopup(hbl: CsTransactionDetail, event: Event, index: number) {
        event.preventDefault();
        event.stopImmediatePropagation();
        event.stopPropagation();

        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Are you sure you want to delete this House Bill, All related charges added to this House Bill will be lost',
            labelCancel: 'No'
        }, () => {
            this.onDeleteHbl();
        });
        this.selectedIndexHBL = index;
        this.selectedHbl = hbl;

    }

    onDeleteHbl() {
        this.deleteHbl(this.selectedHbl.id);
    }

    deleteHbl(id: string) {
        this._documentRepo.deleteHbl(id)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        if (this.selectedIndexHBL > -1) {
                            this.houseBills = [...this.houseBills.slice(0, this.selectedIndexHBL), ...this.houseBills.slice(this.selectedIndexHBL + 1)];
                            if (!!this.houseBills.length) {
                                this.selectHBL(this.houseBills[0]);
                            } else {
                                this.selectedHbl = null;
                                this._store.dispatch(new InitProfitHBLAction())
                            }
                        }
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }

    prepareDeleteJob() {
        this._documentRepo.checkPermissionAllowDeleteShipment(this.jobId)
            .subscribe((value: boolean) => {
                if (value) {
                    this.deleteJob();
                } else {
                    this.showPopupDynamicRender(Permission403PopupComponent, this.viewContainerRef.viewContainerRef, {});
                }
            });
    }

    deleteJob() {
        this._documentRepo.checkMasterBillAllowToDelete(this.jobId)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                            title: 'Alert',
                            body: 'You you sure you want to delete this Job?',
                            labelConfirm: 'Yes'
                        }, () => { this.onDeleteJob(); });
                    } else {
                        this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                            body: 'You are not allowed to delete this job'
                        });
                    }
                },
            );
    }

    onDeleteJob() {
        this._documentRepo.deleteMasterBill(this.jobId)
            .subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {

                        this._toastService.success(respone.message, 'Delete Success !');

                        this.gotoList();
                    }
                },
            );
    }

    onSelectTabSurcharge(tabName: string) {
        this.selectedTabSurcharge = tabName;

        if (!!this.selectedHbl) {
            switch (this.selectedTabSurcharge) {
                case 'BUY':
                    this._store.dispatch(new GetBuyingSurchargeAction({ type: 'BUY', hblId: this.selectedHbl.id }));
                    break;
                case 'SELL':
                    this._store.dispatch(new GetSellingSurchargeAction({ type: 'SELL', hblId: this.selectedHbl.id }));
                    break;
                case 'OBH':
                    this._store.dispatch(new GetOBHSurchargeAction({ type: 'OBH', hblId: this.selectedHbl.id }));
                    break;
                default:
                    break;
            }
        }
    }

    previewPLsheet(currency: string) {
        let hblid = "00000000-0000-0000-0000-000000000000";
        if (!!this.selectedHbl) {
            hblid = this.selectedHbl.id;
        }
        this._documentRepo.previewSIFPLsheet(this.jobId, hblid, currency)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        this.renderAndShowReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    duplicateConfirm() {
        this._documentRepo.getPartnerForCheckPointInShipment(this.jobId, this.transactionType)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                switchMap((partnerIds: string[]) => {
                    if (!!partnerIds.length) {
                        const criteria: DocumentationInterface.ICheckPointCriteria = {
                            data: partnerIds,
                            transactionType: 'DOC',
                            settlementCode: null,
                        };
                        return this._documentRepo.validateCheckPointMultiplePartner(criteria)
                    }
                    return of({ data: null, message: null, status: true });
                })
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.transactionType)}/${this.jobId}`], {
                            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, { action: 'copy' })
                        });
                    }
                }
            )

    }

    gotoList() {
        this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.transactionType)}`]);
    }

    gotoCreate() {
        this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.transactionType)}/${this.jobId}/hbl/new`]);
    }

    gotoDetail(id: string) {
        this._documentRepo.checkDetailShippmentPermission(this.shipmentDetail.id)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.transactionType)}/${this.jobId}/hbl/${id}`]);
                    } else {
                        this.showPopupDynamicRender(Permission403PopupComponent, this.viewContainerRef.viewContainerRef, {});
                    }
                },
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'shipment':
                this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.transactionType)}/${this.jobId}`], { queryParams: { tab: 'SHIPMENT' } });
                break;
            case 'cdNote':
                this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.transactionType)}/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            case 'assignment':
                this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.transactionType)}/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
            case 'files':
                this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.transactionType)}/${this.jobId}`], { queryParams: { tab: 'FILES' } });
                break;
            case 'advance-settle':
                this._router.navigate([`${RoutingConstants.mappingRouteDocumentWithTransactionType(this.transactionType)}/${this.jobId}`], { queryParams: { tab: 'ADVANCE-SETTLE' } });
                break;
        }
    }

    public listenShortcutMovingTab(): void { }

    showMassUpdatePOD() {
        if (!!this.houseBills) {
            this.massUpdatePODComponent.show();
            console.log(this.houseBills);
        }
    }

    renderAndShowReport() {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.viewContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.viewContainerRef.viewContainerRef.clear();
            });

        let sub = ((this.componentRef.instance) as ReportPreviewComponent).onConfirmEdoc
            .pipe(
                concatMap(() => this._exportRepo.exportCrystalReportPDF(this.dataReport, 'response', 'text')),
                mergeMap((res: any) => {
                    if ((res as HttpResponse<any>).status == SystemConstants.HTTP_CODE.OK) {
                        const body = {
                            url: (this.dataReport as Crystal).pathReportGenerate || null,
                            module: 'Document',
                            folder: 'Shipment',
                            objectId: this.jobId,
                            hblId: this.selectedHbl.id,
                            templateCode: 'PLSheet',
                            transactionType: this.selectedHbl.transactionType
                        };
                        return this._fileMngtRepo.uploadPreviewTemplateEdoc([body]);
                    }
                    return of(false);
                }),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (!res) return;
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.success(res.message || "Upload fail");
                    }
                },
                (errors) => {
                    console.log("error", errors);
                },
                () => {
                    sub.unsubscribe();
                }
            );

    }
}

