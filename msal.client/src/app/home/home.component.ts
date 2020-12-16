import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { AuthError, InteractionRequiredAuthError } from 'msal';

const GRAPH_ENDPOINT = 'https://graph.microsoft.com/v1.0/me';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  profile: any;
  weatherForecast: any;

  constructor(private authService: MsalService, private http: HttpClient) { }

  ngOnInit() {
    this.getProfile();
  }

  getProfile() {
    this.http.get(GRAPH_ENDPOINT)
    .subscribe({
      next: (profile) => {
        this.profile = profile;
      },
      error: (err: AuthError) => {
        // If there is an interaction required error,
        // call one of the interactive methods and then make the request again.
        if (InteractionRequiredAuthError.isInteractionRequiredError(err.errorCode)) {
          this.authService.acquireTokenPopup({
            scopes: this.authService.getScopesForEndpoint(GRAPH_ENDPOINT)
          })
          .then(() => {
            this.http.get(GRAPH_ENDPOINT)
              .toPromise()
              .then(profile => {
                this.profile = profile;
              });
          });
        }
      }
    });
  }

  getWeatherForecast() {
    this.http.get('https://localhost:5001/weatherforecast').subscribe(result => {
      this.weatherForecast = result;
    })
  }

}
