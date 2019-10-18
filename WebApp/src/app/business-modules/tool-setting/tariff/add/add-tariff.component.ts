import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { TariffFormAddComponent } from '../components/form-add-tariff/form-add-tariff.component';
import { TariffAdd } from 'src/app/shared/models';
import { SettingRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-add-tariff',
    templateUrl: './add-tariff.component.html',
})
export class TariffAddComponent extends AppList {

    @ViewChild(TariffFormAddComponent, { static: false }) formAddTariffComponent: TariffFormAddComponent;

    tariff: TariffAdd = new TariffAdd();

    constructor(
        private _settingRepo: SettingRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit(): void {
        console.log("khoi tao tariff tạo mới ", this.tariff);
    }

    addTariff() {
        this.formAddTariffComponent.isSubmitted = true;

        if (this.detectInvalidFormAdd()) {
            return;
        }

        if (!this.tariff.setTariffDetails.length) {
            this._toastService.warning('Please add tariff to create new OPS tariff');
            return;
        }

        this.tariff = Object.assign({}, this.tariff, { setTariff: this.formAddTariffComponent.formAdd.value.tariff });
        console.log(this.tariff);

        this._progressRef.start();
        this._settingRepo.addTariff(this.tariff)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    console.log(res);
                },
            );

    }

    detectInvalidFormAdd() {
        return (
            !this.formAddTariffComponent.formAdd.value.tariff.tariffName
            || !this.formAddTariffComponent.formAdd.value.tariff.tariffType
            || !this.formAddTariffComponent.formAdd.value.tariff.status
            || !this.formAddTariffComponent.formAdd.value.tariff.effectiveDate
            || !this.formAddTariffComponent.formAdd.value.tariff.expiredDate
            || !this.formAddTariffComponent.formAdd.value.tariff.productService
            || !this.formAddTariffComponent.formAdd.value.tariff.cargoType
            || !this.formAddTariffComponent.formAdd.value.tariff.serviceMode
            || !this.formAddTariffComponent.selectedOffice.value
        );
    }
}
