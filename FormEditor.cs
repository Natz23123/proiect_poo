using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace proiect_poo
{
    public partial class FormEditor : Form
    {
        // ── State ─────────────────────────────────────────────────────────────
        private StoryJsonDefinition _povesteCurenta;
        private string _caleFichierCurent = "";
        private BlockJsonDefinition _blocCurent;
        private DecisionJsonDefinition _decizieCurenta;
        private IdeaJsonDefinition _ideeCurenta;
        private ResearchLevelJsonDefinition _nivelCurent;
        private ConditionNode _condChildCurent;
        private bool _seIncarcaDatele = false;

        // ── Controale toolbar ─────────────────────────────────────────────────
        private TextBox txtTitluPoveste;
        private Label lblTitluPoveste;
        private Button btnCreazaNoua;

        // ── Stânga – Statusuri ────────────────────────────────────────────────
        private Label lblStatusuriTitlu;
        private ListBox lstStatusuri;
        private TextBox txtStatusNume;
        private Button btnAdaugaStatus, btnStergeStatus, btnStatusSus, btnStatusJos;

        // ── Stânga – TreeView ─────────────────────────────────────────────────
        private Label lblStructuraTitlu;
        private TreeView treeViewStructura;
        private Button btnAdaugaZi, btnStergeZi;
        private Button btnAdaugaBloc;
        private Button btnBlocSus, btnBlocJos;
        private Button btnAdaugaIdee, btnStergeIdee;

        // ── Panel editare BLOC ────────────────────────────────────────────────
        private Panel panelEditareBloc;
        private Label lblBlockIdTitlu, lblBlockTextTitlu;
        private TextBox txtBlockId, txtBlockText;
        private Button btnStergeBloc;

        // Câmpuri tipuri de blocuri
        private Label lblBlockTypeTitlu;
        private ComboBox cmbBlockType;
        private Label lblNextBlockTitlu;
        private TextBox txtNextBlock;

        // Decizii
        private ListBox lstDecizii;
        private Button btnAdaugaDecizie, btnStergeDecizie, btnDecizieSus, btnDecizieJos;

        // Câmpuri decizie
        private Label lblDecizieTextTitlu, lblDecizieDestinatieTitlu;
        private TextBox txtDecizieText, txtDecizieDestinatie;

        // Unlock idee
        private Label lblUnlocksIdee;
        private ComboBox cmbUnlocksIdee;

        // Condiție
        private Label lblCondProp, lblCondOp, lblCondVal;
        private ComboBox cmbCondTip;
        private ComboBox cmbCondProp, cmbCondOp;
        private NumericUpDown numCondVal;

        // Condiție AND/OR – lista de copii
        private Label lblCondCopiiTitlu;
        private ListBox lstCondCopii;
        private Button btnAdaugaCondCopil, btnStergeCondCopil;
        private Label lblCondCopilProp, lblCondCopilOp, lblCondCopilVal;
        private ComboBox cmbCondCopilProp, cmbCondCopilOp;
        private NumericUpDown numCondCopilVal;

        // Efecte
        private Label lblDecizieEfecteTitlu;

        private ListBox lstDecizieEfecte;
        private Button btnAdaugaEfectDecizie, btnStergeEfectDecizie;
        private ComboBox cmbEfectDecizieProp, cmbEfectDecizieTip;
        private NumericUpDown numEfectDecizieVal;

        // ── Panel editare IDEE ────────────────────────────────────────────────
        private Panel panelEditareIdee;
        private Label lblIdeeIdTitlu, lblIdeeNumeTitlu;
        private TextBox txtIdeeId, txtIdeeNume;
        private Label lblNiveluriTitlu;
        private ListBox lstNivele;
        private Button btnAdaugaNivel, btnStergeNivel, btnNivelSus, btnNivelJos;
        private Label lblNivelNrTitlu, lblNivelDescTitlu;

        // Câmpuri editare nivel research
        private NumericUpDown numNivelNr;
        private Label lblNivelEfecteTitlu;
        private ListBox lstNivelEfecte;
        private Button btnAdaugaNivelEfect, btnStergeNivelEfect;
        private ComboBox cmbNivelEfectProp;
        private NumericUpDown numNivelEfectVal;
        private TextBox txtNivelDesc;

        // Imagini
        private Label lblBackgroundImage;
        private Button btnAlegeBackground, btnStergeBackground;
        private PictureBox pbBackgroundPreview;

        private PictureBox pbImgBloc;
        private PictureBox pbIconDecizie;

        // ═════════════════════════════════════════════════════════════════════
        public FormEditor()
        {
            InitializeComponent();
            SetareStareEditare(false);
            panelEditareBloc.Visible = false;
            panelEditareIdee.Visible = false;
        }

        private void SetareStareEditare(bool activa)
        {
            foreach (Control c in new Control[] {
            txtTitluPoveste, lstStatusuri, txtStatusNume,
            btnAdaugaStatus, btnStergeStatus, btnStatusSus, btnStatusJos,
            treeViewStructura, btnAdaugaZi, btnStergeZi,
            btnAdaugaBloc, btnBlocSus, btnBlocJos,
            btnAdaugaIdee, btnStergeIdee,
            btnAlegeBackground, btnStergeBackground })  // ← adăugate
            c.Enabled = activa;
        }

        // ── JSON I/O ──────────────────────────────────────────────────────────
        private void btnCreazaNoua_Click(object sender, EventArgs e)
        {
            _povesteCurenta = new StoryJsonDefinition
            {
                Title = "Poveste Nouă",
                StartBlock = "start_1",
                Properties = new List<PropertyJsonDefinition>(),
                Ideas = new List<IdeaJsonDefinition>(),
                Days = new List<DayJsonDefinition>()
            };
            _povesteCurenta.Properties.Add(new PropertyJsonDefinition
            { Key = "stres", HudLabel = "Stres", Min = 0, Max = 100, Initial = 25, VisibleInHud = true });
            var zi = new DayJsonDefinition { Id = "zi1", Name = "Ziua 1", Blocks = new List<BlockJsonDefinition>() };
            zi.Blocks.Add(new BlockJsonDefinition
            {
                Id = "start_1",
                Text = "Text de start...",
                BlockType = "normal",
                Decisions = new List<DecisionJsonDefinition>()
            });
            _povesteCurenta.Days.Add(zi);
            _caleFichierCurent = "";
            AfiseazaWorkspace();
        }

        private void AfiseazaWorkspace()
        {
            SetareStareEditare(true);
            panelEditareBloc.Visible = false;
            panelEditareIdee.Visible = false;
            _blocCurent = null; _ideeCurenta = null;
            txtTitluPoveste.Text = _povesteCurenta.Title;
            ActualizeazaTreeView();
            ActualizeazaListaStatusuri();
            ActualizeazaBackgroundPreview();
        }

        // export zip
        private void btnExportaZip_Click(object sender, EventArgs e)
        {
            if (_povesteCurenta == null) return;

            string tempRoot = Path.Combine(Path.GetTempPath(), "poveste_export");
            if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
            Directory.CreateDirectory(tempRoot);
            string tempImagini = Path.Combine(tempRoot, "imagini");
            Directory.CreateDirectory(tempImagini);

            var povesteTemp = ClonarePoveste();

            // Copiază imaginea de fundal a poveștii
            if (!string.IsNullOrEmpty(povesteTemp.BackgroundImage))
                povesteTemp.BackgroundImage = CopiazaImagine(povesteTemp.BackgroundImage, tempImagini);

            // Copiază imaginile blocurilor și iconițele deciziilor
            foreach (var zi in povesteTemp.Days)
            {
                foreach (var bloc in zi.Blocks)
                {
                    if (!string.IsNullOrEmpty(bloc.BackgroundImage))
                        bloc.BackgroundImage = CopiazaImagine(bloc.BackgroundImage, tempImagini);

                    if (bloc.Decisions != null)
                    {
                        foreach (var dec in bloc.Decisions)
                        {
                            if (!string.IsNullOrEmpty(dec.Icon))
                                dec.Icon = CopiazaImagine(dec.Icon, tempImagini);
                        }
                    }
                }
            }

            string jsonPath = Path.Combine(tempRoot, "story.json");
            JsonManager.SalveazaPoveste(jsonPath, povesteTemp);

            using (var sfd = new SaveFileDialog { Filter = "Arhivă ZIP (*.zip)|*.zip" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                ZipFile.CreateFromDirectory(tempRoot, sfd.FileName);
            }

            Directory.Delete(tempRoot, true);
            MessageBox.Show("Arhiva ZIP a fost creată!", "Succes");
        }

        //import zip

        private void btnIncarcaZip_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Arhivă ZIP (*.zip)|*.zip" })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;

                string tempRoot = Path.Combine(Path.GetTempPath(), "poveste_import");
                if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
                Directory.CreateDirectory(tempRoot);

                ZipFile.ExtractToDirectory(ofd.FileName, tempRoot);

                string jsonPath = Path.Combine(tempRoot, "story.json");
                if (!File.Exists(jsonPath))
                {
                    MessageBox.Show("Arhiva nu conține story.json!", "Eroare");
                    return;
                }

                _povesteCurenta = JsonManager.IncarcaPoveste(jsonPath);
                _caleFichierCurent = ofd.FileName; // sau memorezi folderul temp

                // Rezolvă căile imaginilor: adaugă prefixul folderului temp/imagini/
                string imagesDir = Path.Combine(tempRoot, "imagini");
                if (Directory.Exists(imagesDir))
                {
                    if (!string.IsNullOrEmpty(_povesteCurenta.BackgroundImage))
                        _povesteCurenta.BackgroundImage = Path.Combine(imagesDir, _povesteCurenta.BackgroundImage);
                    foreach (var zi in _povesteCurenta.Days)
                        foreach (var bloc in zi.Blocks)
                        {
                            if (!string.IsNullOrEmpty(bloc.BackgroundImage))
                                bloc.BackgroundImage = Path.Combine(imagesDir, bloc.BackgroundImage);
                            if (bloc.Decisions != null)
                                foreach (var dec in bloc.Decisions)
                                    if (!string.IsNullOrEmpty(dec.Icon))
                                        dec.Icon = Path.Combine(imagesDir, dec.Icon);
                        }
                }

                AfiseazaWorkspace();
            }
        }

        // ── TreeView ──────────────────────────────────────────────────────────
        private void ActualizeazaTreeView()
        {
            treeViewStructura.Nodes.Clear();

            var nodZile = new TreeNode("📅 Zile & Blocuri") { Tag = "root_zile" };
            foreach (var zi in _povesteCurenta.Days ?? new List<DayJsonDefinition>())
            {
                var nodZi = new TreeNode(zi.Name) { Tag = zi };
                foreach (var b in zi.Blocks ?? new List<BlockJsonDefinition>())
                    nodZi.Nodes.Add(new TreeNode($"[{b.Id}] {Scurt(b.Text, 15)}") { Tag = b });
                nodZile.Nodes.Add(nodZi);
            }
            treeViewStructura.Nodes.Add(nodZile);

            var nodIdei = new TreeNode("💡 Idei") { Tag = "root_idei" };
            foreach (var idee in _povesteCurenta.Ideas ?? new List<IdeaJsonDefinition>())
            {
                var nodIdee = new TreeNode($"[{idee.Id}] {idee.Name}") { Tag = idee };
                foreach (var n in idee.ResearchLevels ?? new List<ResearchLevelJsonDefinition>())
                    nodIdee.Nodes.Add(new TreeNode($"Nivel {n.Level}: {Scurt(n.Description, 20)}") { Tag = n });
                nodIdei.Nodes.Add(nodIdee);
            }
            treeViewStructura.Nodes.Add(nodIdei);

            treeViewStructura.ExpandAll();
        }

        private void treeViewStructura_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var tag = e.Node?.Tag;

            if (tag is BlockJsonDefinition bloc)
            {
                _blocCurent = bloc;
                if (!string.IsNullOrEmpty(_blocCurent.BackgroundImage) && File.Exists(_blocCurent.BackgroundImage))
                {
                    try { pbImgBloc.Image = Image.FromFile(_blocCurent.BackgroundImage); } catch { pbImgBloc.Image = null; }
                }
                else pbImgBloc.Image = null;
                _ideeCurenta = null;
                panelEditareBloc.Visible = true;
                panelEditareIdee.Visible = false;
                _seIncarcaDatele = true;

                txtBlockId.Text = bloc.Id;
                txtBlockText.Text = bloc.Text;
                SelecteazaCombo(cmbBlockType, bloc.BlockType ?? "normal");
                txtNextBlock.Text = bloc.NextBlock ?? "";

                ActualizeazaVizibilitateTipBloc();
                ActualizeazaListaDecizii();
                SetareStareEditareDecizie(false);

                _seIncarcaDatele = false;
                return;
            }

            if (tag is IdeaJsonDefinition idee)
            {
                _ideeCurenta = idee; _blocCurent = null;
                panelEditareBloc.Visible = false;
                panelEditareIdee.Visible = true;
                _seIncarcaDatele = true;
                txtIdeeId.Text = idee.Id;
                txtIdeeNume.Text = idee.Name;
                ActualizeazaListaNivele();
                SetareStareEditareNivel(false);
                _seIncarcaDatele = false;
                return;
            }

            if (tag is ResearchLevelJsonDefinition nivel)
            {
                panelEditareBloc.Visible = false;
                panelEditareIdee.Visible = true;
                if (_ideeCurenta != null)
                {
                    int idx = _ideeCurenta.ResearchLevels.IndexOf(nivel);
                    if (idx >= 0) { _seIncarcaDatele = true; lstNivele.SelectedIndex = idx; _seIncarcaDatele = false; }
                }
                return;
            }

            panelEditareBloc.Visible = false;
            panelEditareIdee.Visible = false;
            _blocCurent = null; _ideeCurenta = null;
        }

        // --Efecte decizie

        private void ActualizeazaListaEfecteDecizie()
        {
            lstDecizieEfecte.Items.Clear();
            if (_decizieCurenta?.Effects == null) return;
            foreach (var ef in _decizieCurenta.Effects)
                lstDecizieEfecte.Items.Add(ef);
        }

        private void SetareStareEditareEfecteDecizie(bool activa)
        {
            lstDecizieEfecte.Enabled = activa;
            btnAdaugaEfectDecizie.Enabled = activa;
            btnStergeEfectDecizie.Enabled = activa;
            cmbEfectDecizieProp.Enabled = activa;
            cmbEfectDecizieTip.Enabled = activa;
            numEfectDecizieVal.Enabled = activa;
        }

        private void btnAdaugaEfectDecizie_Click(object sender, EventArgs e)
        {
            if (_decizieCurenta == null) return;
            string prop = cmbEfectDecizieProp.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(prop))
            {
                MessageBox.Show("Selectează o proprietate!");
                return;
            }
            int val = (int)numEfectDecizieVal.Value;
            string tip = cmbEfectDecizieTip.SelectedItem?.ToString() ?? "ADD";

            var efect = new EffectJsonDefinition
            {
                Type = tip,
                Property = prop,
                Value = val
            };
            _decizieCurenta.Effects.Add(efect);
            ActualizeazaListaEfecteDecizie();
        }

        private void btnStergeEfectDecizie_Click(object sender, EventArgs e)
        {
            if (_decizieCurenta == null || lstDecizieEfecte.SelectedItem == null) return;
            var efect = lstDecizieEfecte.SelectedItem as EffectJsonDefinition;
            if (efect != null)
            {
                _decizieCurenta.Effects.Remove(efect);
                ActualizeazaListaEfecteDecizie();
            }
        }

        private void cmbEfectDecizieProp_SelectedIndexChanged(object sender, EventArgs e) { }
        private void numEfectDecizieVal_ValueChanged(object sender, EventArgs e) { }

        private void ActualizeazaBackgroundPreview()
        {
            if (!string.IsNullOrEmpty(_povesteCurenta?.BackgroundImage) && File.Exists(_povesteCurenta.BackgroundImage))
            {
                try
                {
                    pbBackgroundPreview.Image = Image.FromFile(_povesteCurenta.BackgroundImage);
                }
                catch
                {
                    pbBackgroundPreview.Image = null;
                }
            }
            else
            {
                pbBackgroundPreview.Image = null;
            }
        }

        // ── Statusuri ─────────────────────────────────────────────────────────
        private void ActualizeazaListaStatusuri()
        {
            lstStatusuri.Items.Clear();
            foreach (var p in _povesteCurenta.Properties ?? new List<PropertyJsonDefinition>())
                lstStatusuri.Items.Add(p.Key);
        }
        private void btnAdaugaStatus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStatusNume.Text)) return;
            _povesteCurenta.Properties.Add(new PropertyJsonDefinition
            { Key = txtStatusNume.Text, HudLabel = txtStatusNume.Text, Min = 0, Max = 100, Initial = 0, VisibleInHud = true });
            txtStatusNume.Clear(); ActualizeazaListaStatusuri();
        }
        private void btnStergeStatus_Click(object sender, EventArgs e)
        { int i = lstStatusuri.SelectedIndex; if (i >= 0) { _povesteCurenta.Properties.RemoveAt(i); ActualizeazaListaStatusuri(); } }
        private void btnStatusSus_Click(object sender, EventArgs e) =>
            ReordoneazaLista(_povesteCurenta.Properties, lstStatusuri, -1, ActualizeazaListaStatusuri);
        private void btnStatusJos_Click(object sender, EventArgs e) =>
            ReordoneazaLista(_povesteCurenta.Properties, lstStatusuri, +1, ActualizeazaListaStatusuri);

        // ── Zile & Blocuri ────────────────────────────────────────────────────
        private void btnAdaugaZi_Click(object sender, EventArgs e)
        {
            int n = (_povesteCurenta.Days?.Count ?? 0) + 1;
            _povesteCurenta.Days.Add(new DayJsonDefinition
            { Id = "zi" + n, Name = "Ziua " + n, Blocks = new List<BlockJsonDefinition>() });
            ActualizeazaTreeView();
        }

        private void btnStergeZi_Click(object sender, EventArgs e)
        {
            var nod = treeViewStructura.SelectedNode;
            DayJsonDefinition zi = null;
            if (nod?.Tag is DayJsonDefinition) zi = (DayJsonDefinition)nod.Tag;
            else if (nod?.Parent?.Tag is DayJsonDefinition) zi = (DayJsonDefinition)nod.Parent.Tag;
            if (zi == null) { MessageBox.Show("Selectează o zi din arbore."); return; }
            if (MessageBox.Show($"Ștergi ziua '{zi.Name}' și toate blocurile ei?",
                "Confirmare", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _povesteCurenta.Days.Remove(zi);
            panelEditareBloc.Visible = false;
            ActualizeazaTreeView();
        }

        private void btnAdaugaBloc_Click(object sender, EventArgs e)
        {
            var nod = treeViewStructura.SelectedNode;
            DayJsonDefinition zi = null;
            if (nod?.Tag is DayJsonDefinition) zi = (DayJsonDefinition)nod.Tag;
            else if (nod?.Parent?.Tag is DayJsonDefinition) zi = (DayJsonDefinition)nod.Parent.Tag;
            if (zi == null) { MessageBox.Show("Selectează o zi din arbore!"); return; }
            var bloc = new BlockJsonDefinition
            {
                Id = "bloc_" + Guid.NewGuid().ToString().Substring(0, 4),
                Text = "Text nou...",
                BlockType = "normal",
                Decisions = new List<DecisionJsonDefinition>()
            };
            zi.Blocks.Add(bloc);
            ActualizeazaTreeView(); SelecteazaTag(bloc);
        }

        private void btnStergeBloc_Click(object sender, EventArgs e)
        {
            if (_blocCurent == null) return;
            var zi = _povesteCurenta.Days?.FirstOrDefault(z => z.Blocks.Contains(_blocCurent));
            if (zi == null) return;
            if (MessageBox.Show($"Ștergi blocul '[{_blocCurent.Id}]'?",
                "Confirmare", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            zi.Blocks.Remove(_blocCurent);
            _blocCurent = null;
            panelEditareBloc.Visible = false;
            ActualizeazaTreeView();
        }

        private void btnBlocSus_Click(object sender, EventArgs e)
        {
            if (!(treeViewStructura.SelectedNode?.Tag is BlockJsonDefinition b)) return;
            if (!(treeViewStructura.SelectedNode?.Parent?.Tag is DayJsonDefinition zi)) return;
            int i = zi.Blocks.IndexOf(b);
            if (i > 0) { zi.Blocks.RemoveAt(i); zi.Blocks.Insert(i - 1, b); ActualizeazaTreeView(); SelecteazaTag(b); }
        }
        private void btnBlocJos_Click(object sender, EventArgs e)
        {
            if (!(treeViewStructura.SelectedNode?.Tag is BlockJsonDefinition b)) return;
            if (!(treeViewStructura.SelectedNode?.Parent?.Tag is DayJsonDefinition zi)) return;
            int i = zi.Blocks.IndexOf(b);
            if (i < zi.Blocks.Count - 1) { zi.Blocks.RemoveAt(i); zi.Blocks.Insert(i + 1, b); ActualizeazaTreeView(); SelecteazaTag(b); }
        }

        // ── Editare bloc – câmpuri live ───────────────────────────────────────
        private void txtBlockId_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _blocCurent == null) return;
            _blocCurent.Id = txtBlockId.Text;
            if (treeViewStructura.SelectedNode != null)
                treeViewStructura.SelectedNode.Text = $"[{_blocCurent.Id}] {Scurt(_blocCurent.Text, 15)}";
        }
        private void txtBlockText_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _blocCurent == null) return;
            _blocCurent.Text = txtBlockText.Text;
            if (treeViewStructura.SelectedNode != null)
                treeViewStructura.SelectedNode.Text = $"[{_blocCurent.Id}] {Scurt(_blocCurent.Text, 15)}";
        }
        private void cmbBlockType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _blocCurent == null) return;
            _blocCurent.BlockType = cmbBlockType.SelectedItem?.ToString() ?? "normal";
            ActualizeazaVizibilitateTipBloc();
        }
        private void txtNextBlock_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _blocCurent == null) return;
            _blocCurent.NextBlock = txtNextBlock.Text;
        }

        private void ActualizeazaVizibilitateTipBloc()
        {
            if (_blocCurent == null) return;
            bool esteResearch = (_blocCurent.BlockType == "research");
            lblNextBlockTitlu.Visible = esteResearch;
            txtNextBlock.Visible = esteResearch;
            lstDecizii.Enabled = true;
            btnAdaugaDecizie.Enabled = true;
            btnStergeDecizie.Enabled = true;
        }

        // ── Decizii ───────────────────────────────────────────────────────────
        private void ActualizeazaListaDecizii()
        {
            lstDecizii.Items.Clear();
            if (_blocCurent?.Decisions == null) return;
            for (int i = 0; i < _blocCurent.Decisions.Count; i++)
            {
                var d = _blocCurent.Decisions[i];
                lstDecizii.Items.Add($"Opț.{i + 1}: {Scurt(d.Text, 15)} → [{d.TargetBlock}]");
            }
        }

        private void lstDecizii_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstDecizii.SelectedIndex;
            if (idx < 0 || _blocCurent?.Decisions == null || idx >= _blocCurent.Decisions.Count)
            { 
                _decizieCurenta = null;

                if (!string.IsNullOrEmpty(_decizieCurenta.Icon) && File.Exists(_decizieCurenta.Icon))
                {
                    try { pbIconDecizie.Image = Image.FromFile(_decizieCurenta.Icon); } catch { pbIconDecizie.Image = null; }
                }
                else pbIconDecizie.Image = null;

                SetareStareEditareDecizie(false); return; 
            }

            _decizieCurenta = _blocCurent.Decisions[idx];
            _seIncarcaDatele = true;

            txtDecizieText.Text = _decizieCurenta.Text;
            txtDecizieDestinatie.Text = _decizieCurenta.TargetBlock;
            ActualizeazaListaEfecteDecizie();
            PopuleazaComboProprietati(cmbEfectDecizieProp);

            ActualizeazaComboIdei();
            string uid = _decizieCurenta.UnlocksIdeaId;
            cmbUnlocksIdee.SelectedIndex = string.IsNullOrEmpty(uid) ? 0
                : Math.Max(0, cmbUnlocksIdee.Items.Cast<string>().ToList().IndexOf(uid));

            PopuleazaComboProprietati(cmbCondProp);
            PopuleazaComboProprietati(cmbCondCopilProp);
            IncarcaConditionUI(_decizieCurenta.Condition);

            _seIncarcaDatele = false;
            SetareStareEditareDecizie(true);
        }

        private void SetareStareEditareDecizie(bool activa)
        {
            txtDecizieText.Enabled = activa;
            txtDecizieDestinatie.Enabled = activa;
            SetareStareEditareEfecteDecizie(activa);
            cmbUnlocksIdee.Enabled = activa;
            cmbCondTip.Enabled = activa;
            btnDecizieSus.Enabled = activa;
            btnDecizieJos.Enabled = activa;
            if (!activa) AscundeControleCond();
        }

        private void txtDecizieText_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _decizieCurenta == null) return;
            _decizieCurenta.Text = txtDecizieText.Text;
            _seIncarcaDatele = true; ActualizeazaListaDecizii(); _seIncarcaDatele = false;
        }
        private void txtDecizieDestinatie_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _decizieCurenta == null) return;
            _decizieCurenta.TargetBlock = txtDecizieDestinatie.Text;
            _seIncarcaDatele = true; ActualizeazaListaDecizii(); _seIncarcaDatele = false;
        }


        private void btnAdaugaDecizie_Click(object sender, EventArgs e)
        {
            if (_blocCurent == null) return;
            _blocCurent.Decisions.Add(new DecisionJsonDefinition
            {
                Text = "Opțiune nouă...",
                TargetBlock = "block_id",
                Effects = new List<EffectJsonDefinition>()
            });
            ActualizeazaListaDecizii();
            lstDecizii.SelectedIndex = lstDecizii.Items.Count - 1;
        }
        private void btnStergeDecizie_Click(object sender, EventArgs e)
        {
            int i = lstDecizii.SelectedIndex;
            if (i < 0 || _blocCurent?.Decisions == null) return;
            _blocCurent.Decisions.RemoveAt(i);
            _decizieCurenta = null;
            ActualizeazaListaDecizii();
            SetareStareEditareDecizie(false);
        }
        private void btnDecizieSus_Click(object sender, EventArgs e) =>
            ReordoneazaLista(_blocCurent?.Decisions, lstDecizii, -1, ActualizeazaListaDecizii);
        private void btnDecizieJos_Click(object sender, EventArgs e) =>
            ReordoneazaLista(_blocCurent?.Decisions, lstDecizii, +1, ActualizeazaListaDecizii);

        // ── Unlock idee ───────────────────────────────────────────────────────
        private void ActualizeazaComboIdei()
        {
            cmbUnlocksIdee.Items.Clear();
            cmbUnlocksIdee.Items.Add("(nicio idee)");
            foreach (var idee in _povesteCurenta.Ideas ?? new List<IdeaJsonDefinition>())
                cmbUnlocksIdee.Items.Add(idee.Id);
            cmbUnlocksIdee.SelectedIndex = 0;
        }

        private void cmbUnlocksIdee_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _decizieCurenta == null) return;
            _decizieCurenta.UnlocksIdeaId = cmbUnlocksIdee.SelectedIndex <= 0
                ? null : cmbUnlocksIdee.SelectedItem?.ToString();
        }

        // ── Condiție ──────────────────────────────────────────────────────────
        private void IncarcaConditionUI(ConditionNode cond)
        {
            AscundeControleCond();
            lstCondCopii.Items.Clear();
            _condChildCurent = null;

            if (cond == null)
            {
                cmbCondTip.SelectedIndex = 0;
                return;
            }

            if (cond is ComparisonNode comp)
            {
                cmbCondTip.SelectedIndex = 1;
                AfiseazaControleCond(false);
                _seIncarcaDatele = true;
                SelecteazaCombo(cmbCondProp, comp.Property);
                SelecteazaCombo(cmbCondOp, comp.Operator);
                numCondVal.Value = comp.Value;
                _seIncarcaDatele = false;
            }
            else if (cond is LogicalNode logic)
            {
                cmbCondTip.SelectedIndex = logic.Operator == "AND" ? 2 : 3;
                AfiseazaControleCond(true);
                foreach (var child in logic.Children ?? new List<ConditionNode>())
                    if (child is ComparisonNode c)
                        lstCondCopii.Items.Add($"{c.Property} {c.Operator} {c.Value}");
            }
        }

        private void cmbCondTip_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele) return;
            AscundeControleCond();
            int sel = cmbCondTip.SelectedIndex;
            if (sel == 0) { SalveazaCondition(); return; }
            if (sel == 1) AfiseazaControleCond(false);
            if (sel == 2 || sel == 3) AfiseazaControleCond(true);
            SalveazaCondition();
        }

        private void AfiseazaControleCond(bool esteLogic)
        {
            bool aratSimple = !esteLogic;
            lblCondProp.Visible = cmbCondProp.Visible = aratSimple;
            lblCondOp.Visible = cmbCondOp.Visible = aratSimple;
            lblCondVal.Visible = numCondVal.Visible = aratSimple;

            lblCondCopiiTitlu.Visible = esteLogic;
            lstCondCopii.Visible = esteLogic;
            btnAdaugaCondCopil.Visible = esteLogic;
            btnStergeCondCopil.Visible = esteLogic;
            lblCondCopilProp.Visible = cmbCondCopilProp.Visible = esteLogic;
            lblCondCopilOp.Visible = cmbCondCopilOp.Visible = esteLogic;
            lblCondCopilVal.Visible = numCondCopilVal.Visible = esteLogic;
        }

        private void AscundeControleCond()
        {
            foreach (Control c in new Control[] {
                lblCondProp, cmbCondProp, lblCondOp, cmbCondOp, lblCondVal, numCondVal,
                lblCondCopiiTitlu, lstCondCopii, btnAdaugaCondCopil, btnStergeCondCopil,
                lblCondCopilProp, cmbCondCopilProp, lblCondCopilOp, cmbCondCopilOp,
                lblCondCopilVal, numCondCopilVal })
                c.Visible = false;
        }

        private void SalveazaCondition()
        {
            if (_decizieCurenta == null) return;
            int sel = cmbCondTip.SelectedIndex;

            if (sel == 0) { _decizieCurenta.Condition = null; return; }

            if (sel == 1)
            {
                // Dacă nu e nimic selectat, alege primul status disponibil
                if (cmbCondProp.SelectedIndex < 0 && cmbCondProp.Items.Count > 0)
                    cmbCondProp.SelectedIndex = 0;

                _decizieCurenta.Condition = new ComparisonNode
                {
                    Property = cmbCondProp.SelectedItem?.ToString() ?? "",
                    Operator = cmbCondOp.SelectedItem?.ToString() ?? ">=",
                    Value = (int)numCondVal.Value
                };
                return;
            }

            var logic = new LogicalNode
            {
                Operator = sel == 2 ? "AND" : "OR",
                Children = new List<ConditionNode>()
            };
            foreach (string item in lstCondCopii.Items)
            {
                var parts = item.Split(' ');
                if (parts.Length == 3 && int.TryParse(parts[2], out int v))
                    logic.Children.Add(new ComparisonNode { Property = parts[0], Operator = parts[1], Value = v });
            }
            _decizieCurenta.Condition = logic;
        }

        private void cmbCondProp_SelectedIndexChanged(object sender, EventArgs e)
        { if (!_seIncarcaDatele) SalveazaCondition(); }
        private void cmbCondOp_SelectedIndexChanged(object sender, EventArgs e)
        { if (!_seIncarcaDatele) SalveazaCondition(); }
        private void numCondVal_ValueChanged(object sender, EventArgs e)
        { if (!_seIncarcaDatele) SalveazaCondition(); }

        private void lstCondCopii_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstCondCopii.SelectedIndex;
            if (idx < 0 || !(_decizieCurenta?.Condition is LogicalNode logic) || idx >= logic.Children.Count) return;
            if (logic.Children[idx] is ComparisonNode c)
            {
                _seIncarcaDatele = true;
                SelecteazaCombo(cmbCondCopilProp, c.Property);
                SelecteazaCombo(cmbCondCopilOp, c.Operator);
                numCondCopilVal.Value = c.Value;
                _seIncarcaDatele = false;
                _condChildCurent = c;
            }
        }

        private void btnAdaugaCondCopil_Click(object sender, EventArgs e)
        {
            if (!(_decizieCurenta?.Condition is LogicalNode logic)) return;
            var props = _povesteCurenta.Properties;
            string prop = props?.Count > 0 ? props[0].Key : "stres";
            logic.Children.Add(new ComparisonNode { Property = prop, Operator = ">=", Value = 0 });
            ReincarcaListaCopii();
            lstCondCopii.SelectedIndex = lstCondCopii.Items.Count - 1;
        }

        private void btnStergeCondCopil_Click(object sender, EventArgs e)
        {
            int idx = lstCondCopii.SelectedIndex;
            if (idx < 0 || !(_decizieCurenta?.Condition is LogicalNode logic)) return;
            logic.Children.RemoveAt(idx);
            _condChildCurent = null;
            ReincarcaListaCopii();
        }

        private void cmbCondCopilProp_SelectedIndexChanged(object sender, EventArgs e)
        { if (!_seIncarcaDatele) SalveazaChildCurent(); }
        private void cmbCondCopilOp_SelectedIndexChanged(object sender, EventArgs e)
        { if (!_seIncarcaDatele) SalveazaChildCurent(); }
        private void numCondCopilVal_ValueChanged(object sender, EventArgs e)
        { if (!_seIncarcaDatele) SalveazaChildCurent(); }

        private void SalveazaChildCurent()
        {
            if (!(_condChildCurent is ComparisonNode c)) return;
            c.Property = cmbCondCopilProp.SelectedItem?.ToString() ?? c.Property;
            c.Operator = cmbCondCopilOp.SelectedItem?.ToString() ?? c.Operator;
            c.Value = (int)numCondCopilVal.Value;
            ReincarcaListaCopii();
        }

        private void ReincarcaListaCopii()
        {
            if (!(_decizieCurenta?.Condition is LogicalNode logic)) return;
            int sel = lstCondCopii.SelectedIndex;
            lstCondCopii.Items.Clear();
            foreach (var child in logic.Children)
                if (child is ComparisonNode c)
                    lstCondCopii.Items.Add($"{c.Property} {c.Operator} {c.Value}");
            if (sel < lstCondCopii.Items.Count) lstCondCopii.SelectedIndex = sel;
        }

        // ── Idei ──────────────────────────────────────────────────────────────
        private void btnAdaugaIdee_Click(object sender, EventArgs e)
        {
            if (_povesteCurenta.Ideas == null) _povesteCurenta.Ideas = new List<IdeaJsonDefinition>();
            var ideeNoua = new IdeaJsonDefinition
            {
                Id = "idea_" + Guid.NewGuid().ToString().Substring(0, 4),
                Name = "Idee Nouă",
                ResearchLevels = new List<ResearchLevelJsonDefinition>()
            };
            _povesteCurenta.Ideas.Add(ideeNoua);
            ActualizeazaTreeView(); SelecteazaTag(ideeNoua);
        }

        private void btnStergeIdee_Click(object sender, EventArgs e)
        {
            if (_ideeCurenta == null) return;
            if (MessageBox.Show($"Ștergi ideea '{_ideeCurenta.Name}'?", "Confirmare", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _povesteCurenta.Ideas.Remove(_ideeCurenta);
            _ideeCurenta = null;
            panelEditareIdee.Visible = false;
            ActualizeazaTreeView();
        }

        private void txtIdeeId_TextChanged(object sender, EventArgs e)
        { if (!_seIncarcaDatele && _ideeCurenta != null) { _ideeCurenta.Id = txtIdeeId.Text; ActualizeazaTreeView(); SelecteazaTag(_ideeCurenta); } }
        private void txtIdeeNume_TextChanged(object sender, EventArgs e)
        { if (!_seIncarcaDatele && _ideeCurenta != null) { _ideeCurenta.Name = txtIdeeNume.Text; ActualizeazaTreeView(); SelecteazaTag(_ideeCurenta); } }

        // ── Nivele research ───────────────────────────────────────────────────
        private void ActualizeazaListaNivele()
        {
            lstNivele.Items.Clear();
            foreach (var n in _ideeCurenta?.ResearchLevels ?? new List<ResearchLevelJsonDefinition>())
                lstNivele.Items.Add($"Nivel {n.Level}: {Scurt(n.Description, 25)}");
        }

        private void lstNivele_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstNivele.SelectedIndex;
            if (idx < 0 || _ideeCurenta?.ResearchLevels == null || idx >= _ideeCurenta.ResearchLevels.Count)
            {
                _nivelCurent = null;
                SetareStareEditareNivel(false);
                txtNivelDesc.Text = "";
                return;
            }

            _nivelCurent = _ideeCurenta.ResearchLevels[idx];
            _seIncarcaDatele = true;

            numNivelNr.Value = _nivelCurent.Level;
            txtNivelDesc.Text = _nivelCurent.Description ?? "";

            PopuleazaComboProprietati(cmbNivelEfectProp);
            ActualizeazaListaNivelEfecte();

            _seIncarcaDatele = false;
            SetareStareEditareNivel(true);
        }

        private void SetareStareEditareNivel(bool a)
        {
            numNivelNr.Enabled = txtNivelDesc.Enabled = btnNivelSus.Enabled = btnNivelJos.Enabled = a;
            lstNivelEfecte.Enabled = btnAdaugaNivelEfect.Enabled = btnStergeNivelEfect.Enabled = a;
            cmbNivelEfectProp.Enabled = numNivelEfectVal.Enabled = a;
        }

        private void btnAdaugaNivel_Click(object sender, EventArgs e)
        {
            if (_ideeCurenta == null) return;
            int nou = (_ideeCurenta.ResearchLevels.Count > 0 ? _ideeCurenta.ResearchLevels.Max(n => n.Level) : 0) + 1;
            _ideeCurenta.ResearchLevels.Add(new ResearchLevelJsonDefinition
            {
                Level = nou,
                Description = "Descriere nivel " + nou,
                Effects = new List<EffectJsonDefinition>()
            });
            ActualizeazaListaNivele(); ActualizeazaTreeView(); SelecteazaTag(_ideeCurenta);
            lstNivele.SelectedIndex = lstNivele.Items.Count - 1;
        }
        private void btnStergeNivel_Click(object sender, EventArgs e)
        {
            int i = lstNivele.SelectedIndex;
            if (i < 0) return;
            _ideeCurenta.ResearchLevels.RemoveAt(i); _nivelCurent = null;
            ActualizeazaListaNivele(); ActualizeazaTreeView(); SelecteazaTag(_ideeCurenta);
            SetareStareEditareNivel(false);
        }
        private void btnNivelSus_Click(object sender, EventArgs e) =>
            ReordoneazaLista(_ideeCurenta?.ResearchLevels, lstNivele, -1, ActualizeazaListaNivele);
        private void btnNivelJos_Click(object sender, EventArgs e) =>
            ReordoneazaLista(_ideeCurenta?.ResearchLevels, lstNivele, +1, ActualizeazaListaNivele);

        private void numNivelNr_ValueChanged(object sender, EventArgs e)
        { if (!_seIncarcaDatele && _nivelCurent != null) { _nivelCurent.Level = (int)numNivelNr.Value; ActualizeazaListaNivele(); ActualizeazaTreeView(); SelecteazaTag(_ideeCurenta); } }
        private void txtNivelDesc_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _nivelCurent == null) return;
            _nivelCurent.Description = txtNivelDesc.Text;
            if (treeViewStructura.SelectedNode != null && treeViewStructura.SelectedNode.Tag == _nivelCurent)
                treeViewStructura.SelectedNode.Text = $"Nivel {_nivelCurent.Level}: {Scurt(_nivelCurent.Description, 20)}";
        }

        private void ActualizeazaListaNivelEfecte()
        {
            lstNivelEfecte.Items.Clear();
            if (_nivelCurent == null || _nivelCurent.Effects == null) return;
            foreach (var ef in _nivelCurent.Effects)
                lstNivelEfecte.Items.Add(ef);
        }

        private void lstNivelEfecte_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstNivelEfecte.SelectedIndex;
            if (idx < 0 || _nivelCurent?.Effects == null || idx >= _nivelCurent.Effects.Count) return;
            var ef = _nivelCurent.Effects[idx];
            _seIncarcaDatele = true;
            SelecteazaCombo(cmbNivelEfectProp, ef.Property);
            numNivelEfectVal.Value = ef.Value;
            _seIncarcaDatele = false;
        }

        private void btnAdaugaNivelEfect_Click(object sender, EventArgs e)
        {
            if (_nivelCurent == null)
            {
                MessageBox.Show("Selectează mai întâi un nivel de cercetare!");
                return;
            }
            string propSelectata = cmbNivelEfectProp.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(propSelectata))
            {
                MessageBox.Show("Selectează o proprietate din listă!");
                return;
            }
            int valoare = (int)numNivelEfectVal.Value;
            if (_nivelCurent.Effects == null)
                _nivelCurent.Effects = new List<EffectJsonDefinition>();

            var nouEfect = new EffectJsonDefinition
            {
                Type = "ADD",
                Property = propSelectata,
                Value = valoare
            };
            _nivelCurent.Effects.Add(nouEfect);
            ActualizeazaListaNivelEfecte();
        }

        private void btnStergeNivelEfect_Click(object sender, EventArgs e)
        {
            if (_nivelCurent == null || lstNivelEfecte.SelectedItem == null) return;
            var efSelectat = lstNivelEfecte.SelectedItem as EffectJsonDefinition;
            if (efSelectat != null && _nivelCurent.Effects != null)
            {
                _nivelCurent.Effects.Remove(efSelectat);
                ActualizeazaListaNivelEfecte();
            }
        }

        private void cmbNivelEfectProp_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstNivelEfecte.SelectedIndex;
            if (_seIncarcaDatele || _nivelCurent == null || idx < 0 || idx >= _nivelCurent.Effects.Count) return;
            _nivelCurent.Effects[idx].Property = cmbNivelEfectProp.SelectedItem?.ToString() ?? "";
            _seIncarcaDatele = true;
            ActualizeazaListaNivelEfecte();
            _seIncarcaDatele = false;
        }

        private void numNivelEfectVal_ValueChanged(object sender, EventArgs e)
        {
            int idx = lstNivelEfecte.SelectedIndex;
            if (_seIncarcaDatele || _nivelCurent == null || idx < 0 || idx >= _nivelCurent.Effects.Count) return;
            _nivelCurent.Effects[idx].Value = (int)numNivelEfectVal.Value;
            _seIncarcaDatele = true;
            ActualizeazaListaNivelEfecte();
            _seIncarcaDatele = false;
        }

        //validare poveste

        private void btnValideaza_Click(object sender, EventArgs e)
        {
            var erori = ValideazaPovestea();
            if (erori.Count == 0)
            {
                MessageBox.Show("✅ Povestea este validă! Nu s-au găsit probleme.", "Validare", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string mesaj = "🔴 Au fost găsite următoarele probleme:\n\n";
                foreach (var eroare in erori)
                    mesaj += "• " + eroare + "\n";

                MessageBox.Show(mesaj, "Validare - Probleme găsite", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private StoryJsonDefinition ClonarePoveste()
        {
            // Serializează și deserializează pentru o clonare completă
            string json = JsonConvert.SerializeObject(_povesteCurenta);
            return JsonConvert.DeserializeObject<StoryJsonDefinition>(json);
        }

        private List<string> ValideazaPovestea()
        {
            var erori = new List<string>();
            if (_povesteCurenta == null)
            {
                erori.Add("Nicio poveste încărcată.");
                return erori;
            }

            // Colegem toate ID-urile și proprietățile
            var toateIdBlocuri = new HashSet<string>();
            var toateIdIdei = new HashSet<string>();
            var toateCheiProprietati = new HashSet<string>();

            // Verificăm unicitatea proprietăților și limitele
            foreach (var prop in _povesteCurenta.Properties ?? new List<PropertyJsonDefinition>())
            {
                if (string.IsNullOrEmpty(prop.Key))
                {
                    erori.Add("O proprietate nu are 'key' setat.");
                    continue;
                }
                if (!toateCheiProprietati.Add(prop.Key))
                    erori.Add($"Proprietatea '{prop.Key}' este duplicată.");

                if (prop.Min > prop.Max)
                    erori.Add($"Proprietatea '{prop.Key}': 'min' ({prop.Min}) este mai mare decât 'max' ({prop.Max}).");
                if (prop.Initial < prop.Min || prop.Initial > prop.Max)
                    erori.Add($"Proprietatea '{prop.Key}': 'initial' ({prop.Initial}) nu este în intervalul [{prop.Min}, {prop.Max}].");
            }

            // Colectăm ID-uri de idei
            foreach (var idee in _povesteCurenta.Ideas ?? new List<IdeaJsonDefinition>())
            {
                if (string.IsNullOrEmpty(idee.Id))
                {
                    erori.Add("O idee nu are 'id' setat.");
                    continue;
                }
                if (!toateIdIdei.Add(idee.Id))
                    erori.Add($"ID-ul de idee '{idee.Id}' este duplicat.");
            }

            // Colectăm ID-uri de blocuri
            foreach (var zi in _povesteCurenta.Days ?? new List<DayJsonDefinition>())
            {
                foreach (var bloc in zi.Blocks ?? new List<BlockJsonDefinition>())
                {
                    if (string.IsNullOrEmpty(bloc.Id))
                    {
                        erori.Add("Un bloc nu are 'id' setat.");
                        continue;
                    }
                    if (!toateIdBlocuri.Add(bloc.Id))
                        erori.Add($"ID-ul de bloc '{bloc.Id}' este duplicat.");
                }
            }

            // Verificăm existența blocului de start
            if (string.IsNullOrEmpty(_povesteCurenta.StartBlock))
                erori.Add("Povestea nu are 'startBlock' setat.");
            else if (!toateIdBlocuri.Contains(_povesteCurenta.StartBlock))
                erori.Add($"Blocul de start '{_povesteCurenta.StartBlock}' nu există.");

            // Verificăm fiecare bloc
            foreach (var zi in _povesteCurenta.Days ?? new List<DayJsonDefinition>())
            {
                foreach (var bloc in zi.Blocks ?? new List<BlockJsonDefinition>())
                {
                    if (string.IsNullOrEmpty(bloc.Id)) continue;

                    if (bloc.DecisionsRequired > 0 && (bloc.Decisions == null || bloc.Decisions.Count == 0))
                        erori.Add($"Blocul '{bloc.Id}' necesită {bloc.DecisionsRequired} decizii, dar nu are nicio decizie definită.");

                    if (!string.IsNullOrEmpty(bloc.NextBlock) && !toateIdBlocuri.Contains(bloc.NextBlock))
                        erori.Add($"Blocul '{bloc.Id}': 'nextBlock' '{bloc.NextBlock}' nu există.");

                    if (bloc.Decisions != null)
                    {
                        foreach (var decizie in bloc.Decisions)
                        {
                            if (string.IsNullOrEmpty(decizie.TargetBlock))
                                erori.Add($"Blocul '{bloc.Id}', decizia '{decizie.Text}': nu are 'targetBlock' setat.");
                            else if (!toateIdBlocuri.Contains(decizie.TargetBlock))
                                erori.Add($"Blocul '{bloc.Id}', decizia '{decizie.Text}': blocul destinație '{decizie.TargetBlock}' nu există.");

                            if (!string.IsNullOrEmpty(decizie.UnlocksIdeaId) && !toateIdIdei.Contains(decizie.UnlocksIdeaId))
                                erori.Add($"Blocul '{bloc.Id}', decizia '{decizie.Text}': ideea '{decizie.UnlocksIdeaId}' nu există.");

                            if (decizie.Effects != null)
                                VerificaEfecte(decizie.Effects, toateCheiProprietati, toateIdIdei, $"Blocul '{bloc.Id}', decizia '{decizie.Text}'", erori);

                            if (decizie.Condition != null)
                                VerificaConditionRecursiv(decizie.Condition, toateCheiProprietati, $"Blocul '{bloc.Id}', decizia '{decizie.Text}'", erori);
                        }
                    }
                }
            }

            // Verifică onMaxBlock / onMinBlock
            foreach (var prop in _povesteCurenta.Properties ?? new List<PropertyJsonDefinition>())
            {
                if (!string.IsNullOrEmpty(prop.OnMaxBlock) && !toateIdBlocuri.Contains(prop.OnMaxBlock))
                    erori.Add($"Proprietatea '{prop.Key}': 'onMaxBlock' '{prop.OnMaxBlock}' nu există.");
                if (!string.IsNullOrEmpty(prop.OnMinBlock) && !toateIdBlocuri.Contains(prop.OnMinBlock))
                    erori.Add($"Proprietatea '{prop.Key}': 'onMinBlock' '{prop.OnMinBlock}' nu există.");
            }

            // Verifică ideile
            foreach (var idee in _povesteCurenta.Ideas ?? new List<IdeaJsonDefinition>())
            {
                if (idee.ResearchLevels == null || idee.ResearchLevels.Count == 0)
                    erori.Add($"Ideea '{idee.Id}' nu are niciun nivel de research.");
                else
                {
                    var nivele = new HashSet<int>();
                    foreach (var nivel in idee.ResearchLevels)
                    {
                        if (!nivele.Add(nivel.Level))
                            erori.Add($"Ideea '{idee.Id}' are nivelul {nivel.Level} duplicat.");
                        if (nivel.Effects != null)
                            VerificaEfecte(nivel.Effects, toateCheiProprietati, toateIdIdei, $"Ideea '{idee.Id}', nivelul {nivel.Level}", erori);
                    }
                }
            }

            // Accesibilitate
            var blocuriAccesibile = CalculeazaBlocuriAccesibile(toateIdBlocuri);
            foreach (var id in toateIdBlocuri)
            {
                if (!blocuriAccesibile.Contains(id))
                    erori.Add($"Blocul '{id}' nu este accesibil din 'startBlock' (bloc mort).");
            }

            // Momentan nu verificăm imaginile
            return erori;
        }

        // Verifică o listă de efecte
        private void VerificaEfecte(List<EffectJsonDefinition> efecte, HashSet<string> cheiProprietati, HashSet<string> idIdei, string context, List<string> erori)
        {
            foreach (var efect in efecte)
            {
                if (string.IsNullOrEmpty(efect.Type))
                {
                    erori.Add($"{context}: un efect nu are 'type' setat.");
                    continue;
                }

                if (efect.Type?.ToUpper() == "UNLOCK_LEVEL")
                {
                    if (string.IsNullOrEmpty(efect.Property))
                        erori.Add($"{context}: efectul UNLOCK_LEVEL nu are 'property' (ID-ul ideii).");
                    else if (!idIdei.Contains(efect.Property))
                        erori.Add($"{context}: efectul UNLOCK_LEVEL referă ideea '{efect.Property}' care nu există.");
                }
                else
                {
                    var tipuriValide = new HashSet<string> { "ADD", "SET", "MULTIPLY" };
                    if (!tipuriValide.Contains(efect.Type?.ToUpper()))
                        erori.Add($"{context}: tipul de efect '{efect.Type}' nu este recunoscut (ADD, SET, MULTIPLY).");

                    if (string.IsNullOrEmpty(efect.Property))
                        erori.Add($"{context}: un efect de tip '{efect.Type}' nu are 'property' setat.");
                    else if (!cheiProprietati.Contains(efect.Property))
                        erori.Add($"{context}: efectul folosește proprietatea '{efect.Property}' care nu este definită.");
                }
            }
        }

        private string CopiazaImagine(string caleSursa, string folderDest)
        {
            if (string.IsNullOrEmpty(caleSursa) || !File.Exists(caleSursa))
                return null;
            string numeFisier = Path.GetFileName(caleSursa);
            string dest = Path.Combine(folderDest, numeFisier);
            // Dacă există deja, adaugă un număr
            int i = 1;
            string faraExt = Path.GetFileNameWithoutExtension(numeFisier);
            string ext = Path.GetExtension(numeFisier);
            while (File.Exists(dest))
            {
                dest = Path.Combine(folderDest, $"{faraExt}_{i}{ext}");
                i++;
            }
            File.Copy(caleSursa, dest, true);
            return Path.GetFileName(dest); // returnează doar numele fișierului
        }

        // Verificare recursivă a condițiilor
        private void VerificaConditionRecursiv(ConditionNode cond, HashSet<string> cheiProprietati, string context, List<string> erori)
        {
            if (cond == null) return;

            if (cond is ComparisonNode comp)
            {
                if (string.IsNullOrEmpty(comp.Property))
                    erori.Add($"{context}: condiția nu are 'property' setat.");
                else if (!cheiProprietati.Contains(comp.Property))
                    erori.Add($"{context}: condiția folosește proprietatea '{comp.Property}' care nu este definită.");

                var operatoriValizi = new HashSet<string> { "==", "!=", ">", ">=", "<", "<=" };
                if (string.IsNullOrEmpty(comp.Operator))
                    erori.Add($"{context}: condiția nu are 'operator' setat.");
                else if (!operatoriValizi.Contains(comp.Operator))
                    erori.Add($"{context}: operatorul '{comp.Operator}' nu este valid.");
            }
            else if (cond is LogicalNode logic)
            {
                if (string.IsNullOrEmpty(logic.Operator) || (logic.Operator != "AND" && logic.Operator != "OR"))
                    erori.Add($"{context}: condiția logică are un operator invalid '{logic.Operator}' (așteptat AND sau OR).");

                if (logic.Children == null || logic.Children.Count == 0)
                    erori.Add($"{context}: condiția logică '{logic.Operator}' nu are condiții copil.");
                else
                {
                    foreach (var child in logic.Children)
                        VerificaConditionRecursiv(child, cheiProprietati, context, erori);
                }
            }
        }

        // Calculează blocurile accesibile pornind de la startBlock
        private HashSet<string> CalculeazaBlocuriAccesibile(HashSet<string> toateIdBlocuri)
        {
            var accesibile = new HashSet<string>();
            var coada = new Queue<string>();

            if (!string.IsNullOrEmpty(_povesteCurenta.StartBlock))
                coada.Enqueue(_povesteCurenta.StartBlock);

            while (coada.Count > 0)
            {
                var id = coada.Dequeue();
                if (!accesibile.Add(id)) continue;

                var bloc = GasesteBlocDupaIdSimplu(id);
                if (bloc == null) continue;

                if (!string.IsNullOrEmpty(bloc.NextBlock))
                    coada.Enqueue(bloc.NextBlock);

                if (bloc.Decisions != null)
                {
                    foreach (var decizie in bloc.Decisions)
                    {
                        if (!string.IsNullOrEmpty(decizie.TargetBlock))
                            coada.Enqueue(decizie.TargetBlock);
                    }
                }

                // Adăugăm și blocurile de trigger
                foreach (var prop in _povesteCurenta.Properties ?? new List<PropertyJsonDefinition>())
                {
                    if (!string.IsNullOrEmpty(prop.OnMaxBlock))
                        coada.Enqueue(prop.OnMaxBlock);
                    if (!string.IsNullOrEmpty(prop.OnMinBlock))
                        coada.Enqueue(prop.OnMinBlock);
                }
            }

            return accesibile;
        }

        // Găsește un bloc după ID
        private BlockJsonDefinition GasesteBlocDupaIdSimplu(string idBloc)
        {
            if (_povesteCurenta?.Days == null) return null;
            foreach (var zi in _povesteCurenta.Days)
            {
                var bloc = zi.Blocks.FirstOrDefault(b => b.Id == idBloc);
                if (bloc != null) return bloc;
            }
            return null;
        }

        private void btnAlegeBackground_Click(object sender, EventArgs e)
        {
            if (_povesteCurenta == null)
            {
                MessageBox.Show("Nu există nicio poveste încărcată. Creează sau deschide o poveste mai întâi.",
                                "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var ofd = new OpenFileDialog
            {
                Title = "Alege imaginea de fundal",
                Filter = "Imagini (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
            })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _povesteCurenta.BackgroundImage = ofd.FileName;
                    try
                    {
                        pbBackgroundPreview.Image = Image.FromFile(ofd.FileName);
                    }
                    catch
                    {
                        pbBackgroundPreview.Image = null;
                        MessageBox.Show("Nu s-a putut încărca imaginea selectată.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnStergeBackground_Click(object sender, EventArgs e)
        {
            _povesteCurenta.BackgroundImage = null;
            pbBackgroundPreview.Image = null;
        }

        // ── Utilitare ─────────────────────────────────────────────────────────
        private void ReordoneazaLista<T>(List<T> lista, ListBox lb, int dir, Action refresh)
        {
            if (lista == null) return;
            int i = lb.SelectedIndex, dest = i + dir;
            if (i < 0 || dest < 0 || dest >= lista.Count) return;
            var item = lista[i]; lista.RemoveAt(i); lista.Insert(dest, item);
            refresh(); lb.SelectedIndex = dest;
        }

        private void SelecteazaTag(object tag)
        {
            if (tag == null) return;
            foreach (TreeNode root in treeViewStructura.Nodes)
                foreach (TreeNode child in root.Nodes)
                {
                    if (child.Tag == tag) { treeViewStructura.SelectedNode = child; return; }
                    foreach (TreeNode sub in child.Nodes)
                        if (sub.Tag == tag) { treeViewStructura.SelectedNode = sub; return; }
                }
        }

        private void SelecteazaCombo(ComboBox cmb, string val)
        {
            if (cmb == null) return;
            int idx = cmb.Items.IndexOf(val);
            if (idx >= 0) cmb.SelectedIndex = idx;
            else if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
            else cmb.SelectedIndex = -1;
        }

        private string Scurt(string t, int max)
        {
            if (string.IsNullOrEmpty(t)) return "";
            return t.Length <= max ? t : t.Substring(0, max) + "...";
        }

        private void PopuleazaComboProprietati(ComboBox cmb)
        {
            string sel = cmb.SelectedItem?.ToString();
            cmb.Items.Clear();
            foreach (var p in _povesteCurenta?.Properties ?? new List<PropertyJsonDefinition>())
                cmb.Items.Add(p.Key);
            if (!string.IsNullOrEmpty(sel)) SelecteazaCombo(cmb, sel);
            else if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        // ═════════════════════════════════════════════════════════════════════
        // InitializeComponent
        // ═════════════════════════════════════════════════════════════════════
        private void InitializeComponent()
        {
            //importa zip

            Button btnIncarcaZip = new Button { Location = new Point(148, 12), Size = new Size(130, 30), Text = "📦 Deschide ZIP" };
            btnIncarcaZip.Click += btnIncarcaZip_Click;
            this.Controls.Add(btnIncarcaZip);

            //exporta zip

            Button btnExportaZip = new Button { Location = new Point(280, 12), Size = new Size(130, 30), Text = "📦 Exportă ZIP" };
            btnExportaZip.Click += btnExportaZip_Click;
            this.Controls.Add(btnExportaZip);

            // buton validare

            Button btnValideaza = new Button { Location = new Point(800, 12), Size = new Size(100, 30), Text = "✅ Validează" };
            btnValideaza.Click += btnValideaza_Click;
            this.Controls.Add(btnValideaza);

            // ── Toolbar ──
            btnCreazaNoua = new Button { Location = new Point(12, 12), Size = new Size(130, 30), Text = "➕ Poveste Nouă" };
            lblTitluPoveste = new Label { Location = new Point(580, 17), Size = new Size(60, 20), Text = "Titlu:" };
            txtTitluPoveste = new TextBox { Location = new Point(640, 14), Size = new Size(150, 20) };
            btnCreazaNoua.Click += btnCreazaNoua_Click;

            // ── Stânga — Statusuri ──
            lblStatusuriTitlu = new Label { Location = new Point(12, 55), Size = new Size(220, 18), Text = "📊 Statusuri:" };
            lstStatusuri = new ListBox { Location = new Point(12, 75), Size = new Size(185, 82) };
            btnStatusSus = new Button { Location = new Point(202, 75), Size = new Size(36, 40), Text = "▲" };
            btnStatusJos = new Button { Location = new Point(202, 117), Size = new Size(36, 40), Text = "▼" };
            txtStatusNume = new TextBox { Location = new Point(12, 162), Size = new Size(136, 20) };
            btnAdaugaStatus = new Button { Location = new Point(154, 160), Size = new Size(40, 23), Text = "➕" };
            btnStergeStatus = new Button { Location = new Point(198, 160), Size = new Size(40, 23), Text = "❌" };
            btnStatusSus.Click += btnStatusSus_Click;
            btnStatusJos.Click += btnStatusJos_Click;
            btnAdaugaStatus.Click += btnAdaugaStatus_Click;
            btnStergeStatus.Click += btnStergeStatus_Click;

            // ── Stânga — TreeView ──
            lblStructuraTitlu = new Label { Location = new Point(12, 193), Size = new Size(220, 18), Text = "🌲 Structură:" };
            treeViewStructura = new TreeView { Location = new Point(12, 213), Size = new Size(185, 370) };
            btnBlocSus = new Button { Location = new Point(202, 213), Size = new Size(36, 35), Text = "▲" };
            btnBlocJos = new Button { Location = new Point(202, 251), Size = new Size(36, 35), Text = "▼" };
            btnAdaugaZi = new Button { Location = new Point(12, 592), Size = new Size(85, 26), Text = "📅 + Zi" };
            btnStergeZi = new Button { Location = new Point(102, 592), Size = new Size(95, 26), Text = "🗑 - Zi" };
            btnAdaugaBloc = new Button { Location = new Point(12, 623), Size = new Size(85, 26), Text = "📄 + Bloc" };
            btnAdaugaIdee = new Button { Location = new Point(12, 654), Size = new Size(85, 26), Text = "💡 + Idee" };
            btnStergeIdee = new Button { Location = new Point(102, 654), Size = new Size(95, 26), Text = "🗑 - Idee" };
            treeViewStructura.AfterSelect += treeViewStructura_AfterSelect;
            btnBlocSus.Click += btnBlocSus_Click;
            btnBlocJos.Click += btnBlocJos_Click;
            btnAdaugaZi.Click += btnAdaugaZi_Click;
            btnStergeZi.Click += btnStergeZi_Click;
            btnAdaugaBloc.Click += btnAdaugaBloc_Click;
            btnAdaugaIdee.Click += btnAdaugaIdee_Click;
            btnStergeIdee.Click += btnStergeIdee_Click;

            // ── Panel editare BLOC ──
            panelEditareBloc = new Panel { Location = new Point(248, 55), Size = new Size(810, 690), BorderStyle = BorderStyle.FixedSingle };

            var lblBlocTitlu = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(300, 20),
                Text = "📝 EDITARE BLOC",
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            lblBlockIdTitlu = new Label { Location = new Point(10, 43), Size = new Size(50, 20), Text = "ID Bloc:" };
            txtBlockId = new TextBox { Location = new Point(65, 40), Size = new Size(130, 20) };

            lblBlockTypeTitlu = new Label { Location = new Point(210, 43), Size = new Size(55, 20), Text = "Tip Bloc:" };
            cmbBlockType = new ComboBox { Location = new Point(270, 40), Size = new Size(110, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBlockType.Items.AddRange(new object[] { "normal", "research", "ending", "default_ending" });

            lblNextBlockTitlu = new Label { Location = new Point(395, 43), Size = new Size(75, 20), Text = "Următorul ID:" };
            txtNextBlock = new TextBox { Location = new Point(475, 40), Size = new Size(130, 20) };

            btnStergeBloc = new Button { Location = new Point(685, 38), Size = new Size(110, 24), Text = "🗑 Șterge Bloc" };

            lblBlockTextTitlu = new Label { Location = new Point(10, 73), Size = new Size(200, 15), Text = "Text poveste:" };
            txtBlockText = new TextBox { Location = new Point(10, 91), Size = new Size(785, 110), Multiline = true, ScrollBars = ScrollBars.Vertical };

            txtBlockId.TextChanged += txtBlockId_TextChanged;
            txtBlockText.TextChanged += txtBlockText_TextChanged;
            cmbBlockType.SelectedIndexChanged += cmbBlockType_SelectedIndexChanged;
            txtNextBlock.TextChanged += txtNextBlock_TextChanged;
            btnStergeBloc.Click += btnStergeBloc_Click;

            var lblDecizii = new Label
            {
                Location = new Point(10, 215),
                Size = new Size(200, 20),
                Text = "🔘 Decizii:",
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            lstDecizii = new ListBox { Location = new Point(10, 237), Size = new Size(195, 115) };
            btnDecizieSus = new Button { Location = new Point(210, 237), Size = new Size(36, 55), Text = "▲" };
            btnDecizieJos = new Button { Location = new Point(210, 295), Size = new Size(36, 57), Text = "▼" };
            btnAdaugaDecizie = new Button { Location = new Point(10, 361), Size = new Size(115, 24), Text = "➕ Adaugă Opțiune" };
            btnStergeDecizie = new Button { Location = new Point(130, 361), Size = new Size(115, 24), Text = "❌ Șterge Opțiune" };

            lstDecizii.SelectedIndexChanged += lstDecizii_SelectedIndexChanged;
            btnAdaugaDecizie.Click += btnAdaugaDecizie_Click;
            btnStergeDecizie.Click += btnStergeDecizie_Click;
            btnDecizieSus.Click += btnDecizieSus_Click;
            btnDecizieJos.Click += btnDecizieJos_Click;

            int rx = 262;
            lblDecizieTextTitlu = new Label { Location = new Point(rx, 240), Size = new Size(95, 20), Text = "Text opțiune:" };
            txtDecizieText = new TextBox { Location = new Point(rx + 100, 237), Size = new Size(440, 20) };
            lblDecizieDestinatieTitlu = new Label { Location = new Point(rx, 268), Size = new Size(95, 20), Text = "Sari la bloc ID:" };
            txtDecizieDestinatie = new TextBox { Location = new Point(rx + 100, 265), Size = new Size(440, 20) };

            Button btnAlegeIconDecizie = new Button { Location = new Point(10, 400), Size = new Size(90, 23), Text = "Iconiță" };
            Button btnStergeIconDecizie = new Button { Location = new Point(10 + 95, 400), Size = new Size(80, 23), Text = "Șterge" };
            pbIconDecizie = new PictureBox { Location = new Point(10 + 180, 400), Size = new Size(30, 30), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };

            btnAlegeIconDecizie.Click += (s, ev) =>
            {
                if (_decizieCurenta == null) return;
                using (var ofd = new OpenFileDialog { Filter = "Iconițe (*.png;*.ico)|*.png;*.ico" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        _decizieCurenta.Icon = ofd.FileName;
                        try { pbIconDecizie.Image = Image.FromFile(ofd.FileName); } catch { }
                    }
                }
            };
            btnStergeIconDecizie.Click += (s, ev) =>
            {
                if (_decizieCurenta == null) return;
                _decizieCurenta.Icon = null;
                pbIconDecizie.Image = null;
            };

            panelEditareBloc.Controls.Add(btnAlegeIconDecizie);
            panelEditareBloc.Controls.Add(btnStergeIconDecizie);
            panelEditareBloc.Controls.Add(pbIconDecizie);

            lblUnlocksIdee = new Label { Location = new Point(rx, 295), Size = new Size(95, 20), Text = "Deblochează:" };
            cmbUnlocksIdee = new ComboBox { Location = new Point(rx + 100, 292), Size = new Size(250, 22), DropDownStyle = ComboBoxStyle.DropDownList };

            txtDecizieText.TextChanged += txtDecizieText_TextChanged;
            txtDecizieDestinatie.TextChanged += txtDecizieDestinatie_TextChanged;
            cmbUnlocksIdee.SelectedIndexChanged += cmbUnlocksIdee_SelectedIndexChanged;

            var lblCond = new Label
            {
                Location = new Point(rx, 325),
                Size = new Size(95, 20),
                Text = "Condiție:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            cmbCondTip = new ComboBox { Location = new Point(rx + 100, 322), Size = new Size(160, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCondTip.Items.AddRange(new object[] { "Fără condiție", "Comparație simplă", "AND (mai multe)", "OR (mai multe)" });
            cmbCondTip.SelectedIndex = 0;
            cmbCondTip.SelectedIndexChanged += cmbCondTip_SelectedIndexChanged;

            lblCondProp = new Label { Location = new Point(rx, 352), Size = new Size(55, 20), Text = "Proprietate:", Visible = false };
            cmbCondProp = new ComboBox { Location = new Point(rx + 60, 349), Size = new Size(140, 22), DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            lblCondOp = new Label { Location = new Point(rx + 207, 352), Size = new Size(22, 20), Text = "Op:", Visible = false };
            cmbCondOp = new ComboBox { Location = new Point(rx + 233, 349), Size = new Size(70, 22), DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            cmbCondOp.Items.AddRange(new object[] { "==", "!=", ">", ">=", "<", "<=" });
            lblCondVal = new Label { Location = new Point(rx + 310, 352), Size = new Size(45, 20), Text = "Valoare:", Visible = false };
            numCondVal = new NumericUpDown { Location = new Point(rx + 358, 349), Size = new Size(70, 22), Minimum = -200, Maximum = 200, Visible = false };

            cmbCondProp.SelectedIndexChanged += cmbCondProp_SelectedIndexChanged;
            cmbCondOp.SelectedIndexChanged += cmbCondOp_SelectedIndexChanged;
            numCondVal.ValueChanged += numCondVal_ValueChanged;

            lblCondCopiiTitlu = new Label { Location = new Point(rx, 352), Size = new Size(350, 18), Text = "Condiții (fiecare este o comparație):", Visible = false };
            lstCondCopii = new ListBox { Location = new Point(rx, 372), Size = new Size(350, 80), Visible = false };
            btnAdaugaCondCopil = new Button { Location = new Point(rx + 357, 372), Size = new Size(80, 38), Text = "➕ Add", Visible = false };
            btnStergeCondCopil = new Button { Location = new Point(rx + 357, 413), Size = new Size(80, 39), Text = "❌ Del", Visible = false };

            lblCondCopilProp = new Label { Location = new Point(rx, 460), Size = new Size(55, 20), Text = "Proprietate:", Visible = false };
            cmbCondCopilProp = new ComboBox { Location = new Point(rx + 60, 457), Size = new Size(140, 22), DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            lblCondCopilOp = new Label { Location = new Point(rx + 207, 460), Size = new Size(22, 20), Text = "Op:", Visible = false };
            cmbCondCopilOp = new ComboBox { Location = new Point(rx + 233, 457), Size = new Size(70, 22), DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            cmbCondCopilOp.Items.AddRange(new object[] { "==", "!=", ">", ">=", "<", "<=" });
            lblCondCopilVal = new Label { Location = new Point(rx + 310, 460), Size = new Size(45, 20), Text = "Valoare:", Visible = false };
            numCondCopilVal = new NumericUpDown { Location = new Point(rx + 358, 457), Size = new Size(70, 22), Minimum = -200, Maximum = 200, Visible = false };

            lstCondCopii.SelectedIndexChanged += lstCondCopii_SelectedIndexChanged;
            btnAdaugaCondCopil.Click += btnAdaugaCondCopil_Click;
            btnStergeCondCopil.Click += btnStergeCondCopil_Click;
            cmbCondCopilProp.SelectedIndexChanged += cmbCondCopilProp_SelectedIndexChanged;
            cmbCondCopilOp.SelectedIndexChanged += cmbCondCopilOp_SelectedIndexChanged;
            numCondCopilVal.ValueChanged += numCondCopilVal_ValueChanged;

            lblDecizieEfecteTitlu = new Label { Location = new Point(rx, 492), Size = new Size(300, 15), Text = "Efecte:" };
            lstDecizieEfecte = new ListBox { Location = new Point(rx, 510), Size = new Size(250, 120) };
            btnAdaugaEfectDecizie = new Button { Location = new Point(rx + 260, 510), Size = new Size(80, 24), Text = "➕ Adaugă" };
            btnStergeEfectDecizie = new Button { Location = new Point(rx + 260, 538), Size = new Size(80, 24), Text = "❌ Șterge" };

            cmbEfectDecizieTip = new ComboBox { Location = new Point(rx, 640), Size = new Size(80, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEfectDecizieTip.Items.AddRange(new object[] { "ADD", "SET", "MULTIPLY" });
            cmbEfectDecizieTip.SelectedIndex = 0;

            cmbEfectDecizieProp = new ComboBox { Location = new Point(rx + 90, 640), Size = new Size(130, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            numEfectDecizieVal = new NumericUpDown { Location = new Point(rx + 230, 640), Size = new Size(70, 22), Minimum = -200, Maximum = 200 };

            btnAdaugaEfectDecizie.Click += btnAdaugaEfectDecizie_Click;
            btnStergeEfectDecizie.Click += btnStergeEfectDecizie_Click;
            cmbEfectDecizieProp.SelectedIndexChanged += cmbEfectDecizieProp_SelectedIndexChanged;
            numEfectDecizieVal.ValueChanged += numEfectDecizieVal_ValueChanged;

            // ── Imagine de fundal ──
            lblBackgroundImage = new Label { Location = new Point(12, 690), Size = new Size(130, 20), Text = "Imagine fundal:" };
            btnAlegeBackground = new Button { Location = new Point(12, 712), Size = new Size(100, 23), Text = "Alege imagine" };
            btnStergeBackground = new Button { Location = new Point(115, 712), Size = new Size(80, 23), Text = "Șterge" };
            pbBackgroundPreview = new PictureBox { Location = new Point(12, 745), Size = new Size(80, 60), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };

            btnAlegeBackground.Click += btnAlegeBackground_Click;
            btnStergeBackground.Click += btnStergeBackground_Click;

            this.Controls.Add(lblBackgroundImage);
            this.Controls.Add(btnAlegeBackground);
            this.Controls.Add(btnStergeBackground);
            this.Controls.Add(pbBackgroundPreview);

            // Imagine fundal pentru bloc
            Button btnAlegeImgBloc = new Button { Location = new Point(10, 210), Size = new Size(130, 23), Text = "Imagine fundal bloc" };
            Button btnStergeImgBloc = new Button { Location = new Point(145, 210), Size = new Size(80, 23), Text = "Șterge" };
            pbImgBloc = new PictureBox { Location = new Point(230, 205), Size = new Size(40, 30), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };

            btnAlegeImgBloc.Click += (s, ev) =>
            {
                if (_blocCurent == null) return;
                using (var ofd = new OpenFileDialog { Filter = "Imagini (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        _blocCurent.BackgroundImage = ofd.FileName;
                        try { pbImgBloc.Image = Image.FromFile(ofd.FileName); } catch { }
                    }
                }
            };
            btnStergeImgBloc.Click += (s, ev) =>
            {
                if (_blocCurent == null) return;
                _blocCurent.BackgroundImage = null;
                pbImgBloc.Image = null;
            };

            panelEditareBloc.Controls.Add(btnAlegeImgBloc);
            panelEditareBloc.Controls.Add(btnStergeImgBloc);
            panelEditareBloc.Controls.Add(pbImgBloc);

            // Adăugăm controalele la panou
            panelEditareBloc.Controls.Add(lblDecizieEfecteTitlu);
            panelEditareBloc.Controls.Add(lstDecizieEfecte);
            panelEditareBloc.Controls.Add(btnAdaugaEfectDecizie);
            panelEditareBloc.Controls.Add(btnStergeEfectDecizie);
            panelEditareBloc.Controls.Add(cmbEfectDecizieTip);
            panelEditareBloc.Controls.Add(cmbEfectDecizieProp);
            panelEditareBloc.Controls.Add(numEfectDecizieVal);

            panelEditareBloc.Controls.Add(lblBlocTitlu);
            panelEditareBloc.Controls.Add(lblBlockIdTitlu);
            panelEditareBloc.Controls.Add(txtBlockId);
            panelEditareBloc.Controls.Add(lblBlockTypeTitlu);
            panelEditareBloc.Controls.Add(cmbBlockType);
            panelEditareBloc.Controls.Add(lblNextBlockTitlu);
            panelEditareBloc.Controls.Add(txtNextBlock);
            panelEditareBloc.Controls.Add(btnStergeBloc);
            panelEditareBloc.Controls.Add(lblBlockTextTitlu);
            panelEditareBloc.Controls.Add(txtBlockText);
            panelEditareBloc.Controls.Add(lblDecizii);
            panelEditareBloc.Controls.Add(lstDecizii);
            panelEditareBloc.Controls.Add(btnDecizieSus);
            panelEditareBloc.Controls.Add(btnDecizieJos);
            panelEditareBloc.Controls.Add(btnAdaugaDecizie);
            panelEditareBloc.Controls.Add(btnStergeDecizie);
            panelEditareBloc.Controls.Add(lblDecizieTextTitlu);
            panelEditareBloc.Controls.Add(txtDecizieText);
            panelEditareBloc.Controls.Add(lblDecizieDestinatieTitlu);
            panelEditareBloc.Controls.Add(txtDecizieDestinatie);
            panelEditareBloc.Controls.Add(lblUnlocksIdee);
            panelEditareBloc.Controls.Add(cmbUnlocksIdee);
            panelEditareBloc.Controls.Add(lblCond);
            panelEditareBloc.Controls.Add(cmbCondTip);
            panelEditareBloc.Controls.Add(lblCondProp);
            panelEditareBloc.Controls.Add(cmbCondProp);
            panelEditareBloc.Controls.Add(lblCondOp);
            panelEditareBloc.Controls.Add(cmbCondOp);
            panelEditareBloc.Controls.Add(lblCondVal);
            panelEditareBloc.Controls.Add(numCondVal);
            panelEditareBloc.Controls.Add(lblCondCopiiTitlu);
            panelEditareBloc.Controls.Add(lstCondCopii);
            panelEditareBloc.Controls.Add(btnAdaugaCondCopil);
            panelEditareBloc.Controls.Add(btnStergeCondCopil);
            panelEditareBloc.Controls.Add(lblCondCopilProp);
            panelEditareBloc.Controls.Add(cmbCondCopilProp);
            panelEditareBloc.Controls.Add(lblCondCopilOp);
            panelEditareBloc.Controls.Add(cmbCondCopilOp);
            panelEditareBloc.Controls.Add(lblCondCopilVal);
            panelEditareBloc.Controls.Add(numCondCopilVal);
            panelEditareBloc.Controls.Add(lblDecizieEfecteTitlu);

            // ── Panel editare IDEE ──
            panelEditareIdee = new Panel { Location = new Point(248, 55), Size = new Size(810, 690), BorderStyle = BorderStyle.FixedSingle };

            var lblIdeeTitlu = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(400, 20),
                Text = "💡 EDITARE IDEE",
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            lblIdeeIdTitlu = new Label { Location = new Point(10, 43), Size = new Size(55, 20), Text = "ID Idee:" };
            txtIdeeId = new TextBox { Location = new Point(70, 40), Size = new Size(250, 20) };
            lblIdeeNumeTitlu = new Label { Location = new Point(10, 73), Size = new Size(50, 20), Text = "Nume:" };
            txtIdeeNume = new TextBox { Location = new Point(70, 70), Size = new Size(250, 20) };
            lblNiveluriTitlu = new Label
            {
                Location = new Point(10, 105),
                Size = new Size(300, 20),
                Text = "🔬 Nivele Research:",
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            lstNivele = new ListBox { Location = new Point(10, 127), Size = new Size(220, 180) };
            btnNivelSus = new Button { Location = new Point(236, 127), Size = new Size(36, 88), Text = "▲" };
            btnNivelJos = new Button { Location = new Point(236, 218), Size = new Size(36, 89), Text = "▼" };
            btnAdaugaNivel = new Button { Location = new Point(10, 315), Size = new Size(110, 25), Text = "➕ Nivel Nou" };
            btnStergeNivel = new Button { Location = new Point(126, 315), Size = new Size(110, 25), Text = "❌ Șterge Nivel" };

            lblNivelEfecteTitlu = new Label { Location = new Point(290, 170), Size = new Size(200, 20), Text = "Efecte asupra statusurilor:" };
            lstNivelEfecte = new ListBox { Location = new Point(290, 195), Size = new Size(220, 95) };
            btnAdaugaNivelEfect = new Button { Location = new Point(520, 195), Size = new Size(80, 24), Text = "➕ Adaugă" };
            btnStergeNivelEfect = new Button { Location = new Point(520, 223), Size = new Size(80, 24), Text = "❌ Șterge" };
            cmbNivelEfectProp = new ComboBox { Location = new Point(290, 300), Size = new Size(130, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            numNivelEfectVal = new NumericUpDown { Location = new Point(430, 300), Size = new Size(70, 22), Minimum = -100, Maximum = 100 };

            txtIdeeId.TextChanged += txtIdeeId_TextChanged;
            txtIdeeNume.TextChanged += txtIdeeNume_TextChanged;
            lstNivele.SelectedIndexChanged += lstNivele_SelectedIndexChanged;
            btnAdaugaNivel.Click += btnAdaugaNivel_Click;
            btnStergeNivel.Click += btnStergeNivel_Click;
            btnNivelSus.Click += btnNivelSus_Click;
            btnNivelJos.Click += btnNivelJos_Click;

            lblNivelNrTitlu = new Label { Location = new Point(330, 43), Size = new Size(70, 20), Text = "Număr Nivel:" };
            numNivelNr = new NumericUpDown { Location = new Point(400, 40), Size = new Size(60, 22), Minimum = 0 };

            lblNivelDescTitlu = new Label { Location = new Point(330, 73), Size = new Size(70, 20), Text = "Descriere:" };
            txtNivelDesc = new TextBox
            {
                Location = new Point(400, 70),
                Size = new Size(400, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            btnAdaugaNivelEfect.Click += btnAdaugaNivelEfect_Click;
            btnStergeNivelEfect.Click += btnStergeNivelEfect_Click;
            cmbNivelEfectProp.SelectedIndexChanged += cmbNivelEfectProp_SelectedIndexChanged;
            numNivelEfectVal.ValueChanged += numNivelEfectVal_ValueChanged;

            // Evenimentele pentru noile controale
            txtNivelDesc.TextChanged += txtNivelDesc_TextChanged;
            numNivelNr.ValueChanged += numNivelNr_ValueChanged;

            panelEditareIdee.Controls.Add(lblIdeeTitlu);
            panelEditareIdee.Controls.Add(lblIdeeIdTitlu);
            panelEditareIdee.Controls.Add(txtIdeeId);
            panelEditareIdee.Controls.Add(lblIdeeNumeTitlu);
            panelEditareIdee.Controls.Add(txtIdeeNume);
            panelEditareIdee.Controls.Add(lblNiveluriTitlu);
            panelEditareIdee.Controls.Add(lstNivele);
            panelEditareIdee.Controls.Add(btnNivelSus);
            panelEditareIdee.Controls.Add(btnNivelJos);
            panelEditareIdee.Controls.Add(btnAdaugaNivel);
            panelEditareIdee.Controls.Add(btnStergeNivel);
            panelEditareIdee.Controls.Add(lblNivelEfecteTitlu);
            panelEditareIdee.Controls.Add(lstNivelEfecte);
            panelEditareIdee.Controls.Add(btnAdaugaNivelEfect);
            panelEditareIdee.Controls.Add(btnStergeNivelEfect);
            panelEditareIdee.Controls.Add(cmbNivelEfectProp);
            panelEditareIdee.Controls.Add(numNivelEfectVal);

            // Adăugăm și controalele noi
            panelEditareIdee.Controls.Add(lblNivelNrTitlu);
            panelEditareIdee.Controls.Add(numNivelNr);
            panelEditareIdee.Controls.Add(lblNivelDescTitlu);
            panelEditareIdee.Controls.Add(txtNivelDesc);

            // ── Form ──
            this.SuspendLayout();
            this.ClientSize = new Size(1070, 820);
            this.Text = "Story Editor";

            this.Controls.Add(btnCreazaNoua);
            this.Controls.Add(lblTitluPoveste);
            this.Controls.Add(txtTitluPoveste);
            this.Controls.Add(lblStatusuriTitlu);
            this.Controls.Add(lstStatusuri);
            this.Controls.Add(btnStatusSus);
            this.Controls.Add(btnStatusJos);
            this.Controls.Add(txtStatusNume);
            this.Controls.Add(btnAdaugaStatus);
            this.Controls.Add(btnStergeStatus);
            this.Controls.Add(lblStructuraTitlu);
            this.Controls.Add(treeViewStructura);
            this.Controls.Add(btnBlocSus);
            this.Controls.Add(btnBlocJos);
            this.Controls.Add(btnAdaugaZi);
            this.Controls.Add(btnStergeZi);
            this.Controls.Add(btnAdaugaBloc);
            this.Controls.Add(btnAdaugaIdee);
            this.Controls.Add(btnStergeIdee);
            this.Controls.Add(panelEditareBloc);
            this.Controls.Add(panelEditareIdee);

            this.ResumeLayout(false);
        }
    }
}