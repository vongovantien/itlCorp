import { Component, ViewChild } from '@angular/core';
import { TariffAddComponent } from '../add/add-tariff.component';
import { SettingRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { switchMap, tap, catchError, finalize } from 'rxjs/operators';
import { TariffAdd } from 'src/app/shared/models';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'app-detail-tariff',
    templateUrl: './detail-tariff.component.html',
})
export class TariffDetailComponent extends TariffAddComponent {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    tariffId: string = '';
    tariff: TariffAdd;

    constructor(
        protected _settingRepo: SettingRepo,
        protected _progressService: NgProgress,
        protected _toastService: ToastrService,
        private _activedRoute: ActivatedRoute,
        private _router: Router
    ) {
        super(_settingRepo, _progressService, _toastService);
        this._progressRef = this._progressService.ref();
    }

    ngOnInit(): void {
    }

    ngAfterViewInit() {
        this._progressRef.start();
        this._activedRoute.params
            .pipe(
                tap((param: Params) => {
                    this.tariffId = param.id;
                }),
                switchMap(
                    () => this._settingRepo.getDetailTariff(this.tariffId)
                        .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                ),
            )
            .subscribe((res: CommonInterface.IResult) => {
                if (res.message) {
                    this.tariff = new TariffAdd(res.data);
                    console.log(this.tariff);

                    const objectTariffForm = {
                        tariffName: this.tariff.setTariff.tariffName,
                        productService: (this.formAddTariffComponent.productSerices.filter(i => i.value === this.tariff.setTariff.productService))[0],
                        tariffType: this.formAddTariffComponent.tariffTypes.filter(type => type.value === this.tariff.setTariff.tariffType)[0],
                        status: this.formAddTariffComponent.status.filter(i => i.value === this.tariff.setTariff.status)[0],
                        effectiveDate: { startDate: new Date(this.tariff.setTariff.effectiveDate), endDate: new Date(this.tariff.setTariff.effectiveDate) },
                        expiredDate: { startDate: new Date(this.tariff.setTariff.expiredDate), endDate: new Date(this.tariff.setTariff.expiredDate) },
                        cargoType: this.formAddTariffComponent.cargoTypes.filter(type => type.value === this.tariff.setTariff.cargoType)[0],
                        serviceMode: this.formAddTariffComponent.serviceModes.filter(mode => mode.value === this.tariff.setTariff.serviceMode)[0],
                        description: this.tariff.setTariff.description,
                    };

                    // * Update Form Group.
                    this.formAddTariffComponent.formAdd.controls['tariff'].setValue(objectTariffForm);

                    // * Update comboGrid.
                    this.formAddTariffComponent.selectedOffice = <CommonInterface.IComboGridData>{ field: 'id', value: this.tariff.setTariff.officeId };

                    switch (this.tariff.setTariff.tariffType) {
                        case 'Customer':
                            this.formAddTariffComponent.selectedCustomer = <CommonInterface.IComboGridData>{ field: 'id', value: this.tariff.setTariff.customerId };

                            break;
                        case 'Supplier':
                            this.formAddTariffComponent.selectedSupplier = <CommonInterface.IComboGridData>{ field: 'id', value: this.tariff.setTariff.supplierId };

                            break;
                        case 'Agent':
                            this.formAddTariffComponent.selectedAgent = <CommonInterface.IComboGridData>{ field: 'id', value: this.tariff.setTariff.agentId };
                            break;
                        default:

                            break;
                    }

                    this.formAddTariffComponent.onChangeTariffType({ value: this.tariff.setTariff.tariffType, displayName: '' });
                }
            });
    }

    updateTafiff() {
        if (!this.checkValidateFormTariff()) {
            return;
        }
        this.onSubmitData();

        // * UPDATE ID FOR CREATE TARIFF.
        this.tariff.setTariff.id = this.tariffId;
        for (const item of this.tariff.setTariffDetails) {
            item.tariffId = this.tariffId;
        }

        this._progressRef.start();
        this._settingRepo.updateTariff(this.tariff)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error(res.message);
                    }
                },
            );
    }

    deleteTariff() {
        this.confirmDeletePopup.show();
    }

    onDeleteTariff() {
        this.confirmDeletePopup.show();
        this._progressRef.start();
        this._settingRepo.deleteTariff(this.tariffId)
            .pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.confirmDeletePopup.hide();
            }))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._router.navigate(["home/tool/tariff"]);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

}
