﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RestSharp;
using System.Net;
using System.Xml.Linq;

//example text/xml response: http://www.w3schools.com/xml/note.asp
namespace Swensen.RestSharpGui
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            bindHttpMethods();            
            ActiveControl = txtUrl;
        }

        private void bindHttpMethods() {
            rbHttpDelete.Tag = Method.DELETE;
            rbHttpGet.Tag = Method.GET;
            rbHttpHead.Tag = Method.HEAD;
            rbHttpOptions.Tag = Method.OPTIONS;
            rbHttpPatch.Tag = Method.PATCH;
            rbHttpPost.Tag = Method.POST;
            rbHttpPut.Tag = Method.PUT;
        }

        private IEnumerable<RadioButton> rbGrpHttpMethods { get { return grpHttpMethod.Controls.OfType<RadioButton>(); } }

        private void btnSubmitRequest_Click(object sender, EventArgs e)
        {
            //build the request view
            var checkedHttpMethod = rbGrpHttpMethods.Where(x => x.Checked).FirstOrDefault();
            var requestVm = new RequestViewModel() {
                Url = txtUrl.Text,
                Method = checkedHttpMethod == null ? null : ((Method?)checkedHttpMethod.Tag),
                Headers = txtRequestHeaders.Lines.ToArray(),
                Body = txtRequestBody.Text
            };

            //attempt to build the request model from the request view: if we fail, show an error messages, else continue and make the request.
            RequestModel requestModel = null;
            var validationErrors = RequestModel.TryCreate(requestVm, out requestModel);
            if (validationErrors.Count > 0)
                MessageBox.Show(this, String.Join(Environment.NewLine, validationErrors), "Request Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else {
                //clear response view and show loading message status
                bind(new ResponseViewModel(responseStatus:"Loading..."));
                grpResponse.Update();

                //execute the request and get the response
                var restRequest = requestModel.ToRestRequest();
                var client = new RestClient();
                var restResponse = client.Execute(restRequest);
                var responseVm = new ResponseViewModel(restResponse);

                //bind the response view
                bind(responseVm);
                grpResponse.Update();
            }
        }

        private void bind(ResponseViewModel responseVm) {
            lblResponseStatusValue.Text = responseVm.ResponseStatus;
            rtResponseText.Text = responseVm.PrettyPrintedContent;
            txtResponseHeaders.Text = responseVm.Headers;            
        }

        private void bind(RequestViewModel requestVm) {
            txtUrl.Text = requestVm.Url;
            
            var method = requestVm.Method ?? Method.GET;
            rbGrpHttpMethods.First(x => ((Method) x.Tag) == method).Checked = true;

            txtRequestHeaders.Lines = (requestVm.Headers ?? new string[0]).ToArray();
            txtRequestBody.Text = requestVm.Body;
        }

        private void btnClearRequest_Click(object sender, EventArgs e)
        {
            bind(new RequestViewModel());
            bind(new ResponseViewModel(responseStatus:""));
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }
    }
}
