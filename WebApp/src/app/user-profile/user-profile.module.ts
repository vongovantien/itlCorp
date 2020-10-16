import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { UserProfilePageComponent } from './user-profile.component';

const routing: Routes = [
    {
        path: ':id',
        component: UserProfilePageComponent,
        data: { name: '' },
    },

];


@NgModule({
    declarations: [
        UserProfilePageComponent,
    ],
    imports: [
        CommonModule,
        TabsModule.forRoot(),
        RouterModule.forChild(routing),
        SharedModule,
        ReactiveFormsModule,
    ],
    exports: [],
    providers: [],
})
export class UserProfileModule { }
